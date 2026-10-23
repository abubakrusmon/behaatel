import { expect, test, type Page } from '@playwright/test'
import { mkdir, readFile } from 'node:fs/promises'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const testDirectory = path.dirname(fileURLToPath(import.meta.url))
const screenshotsDirectory = path.resolve('e2e-screenshots')
const credentials = JSON.parse(
  await readFile(path.join(testDirectory, '.e2e-credentials.json'), 'utf8'),
) as { email: string; password: string; newPassword: string }
const residentEmail = credentials.email
const initialResidentPassword = credentials.password
const updatedResidentPassword = credentials.newPassword
const adminEmail = process.env.E2E_ADMIN_EMAIL ?? 'superadmin@behaatel.com'
const adminPassword = process.env.E2E_ADMIN_PASSWORD

if (!residentEmail || !initialResidentPassword || !updatedResidentPassword || !adminPassword) {
  throw new Error('Resident credentials file and E2E_ADMIN_PASSWORD environment variable are required.')
}

interface BrowserAudit {
  consoleErrors: string[]
  pageErrors: string[]
  apiErrors: string[]
}

const audits = new WeakMap<Page, BrowserAudit>()

test.beforeEach(async ({ page }) => {
  const audit: BrowserAudit = { consoleErrors: [], pageErrors: [], apiErrors: [] }
  audits.set(page, audit)
  page.on('console', (message) => {
    if (message.type() === 'error') audit.consoleErrors.push(message.text())
  })
  page.on('pageerror', (error) => audit.pageErrors.push(error.message))
  page.on('response', (response) => {
    if (!response.url().startsWith('http://localhost:5000/api/v1/')) return
    if (response.status() < 400) return
    const request = response.request()
    const expectedInvalidLogin = test.info().title === 'login screen reports invalid password clearly'
      && request.method() === 'POST'
      && response.url().endsWith('/auth/login')
      && response.status() === 401
    if (!expectedInvalidLogin) audit.apiErrors.push(`${request.method()} ${response.url()} -> ${response.status()}`)
  })
  page.on('requestfailed', (request) => {
    if (request.url().startsWith('http://localhost:5000/api/v1/')) {
      audit.apiErrors.push(`${request.method()} ${request.url()} failed: ${request.failure()?.errorText ?? 'unknown error'}`)
    }
  })
})

test.afterEach(async ({ page }, testInfo) => {
  const audit = audits.get(page)
  await page.setViewportSize({ width: 390, height: 844 })
  await page.waitForTimeout(100)
  const horizontalOverflow = await page.evaluate(() => (
    document.documentElement.scrollWidth > window.innerWidth
    || document.body.scrollWidth > window.innerWidth
  ))
  expect(horizontalOverflow, 'mobile page must not scroll horizontally').toBe(false)
  expect(audit?.consoleErrors ?? [], 'browser console.error messages').toEqual([])
  expect(audit?.pageErrors ?? [], 'uncaught browser exceptions').toEqual([])
  expect(audit?.apiErrors ?? [], 'unexpected API errors or failed requests').toEqual([])
  if (testInfo.status !== testInfo.expectedStatus) {
    await page.screenshot({ path: testInfo.outputPath('failure.png'), fullPage: true }).catch(() => undefined)
  }
})

test('login screen reports invalid password clearly', async ({ page }) => {
  await page.goto('/login')
  await expect(page.getByRole('heading', { name: 'Добро пожаловать' })).toBeVisible()
  await page.getByLabel('Электронная почта').fill(residentEmail)
  await page.getByLabel('Пароль').fill('DefinitelyWrong123!')
  await page.getByRole('button', { name: 'Войти' }).click()
  await expect(page.getByRole('alert')).toContainText(/Неверная|неверн/i)
})

test('resident signs in, changes the initial password, and reaches home', async ({ page }) => {
  await loginResident(page, initialResidentPassword)
  await expect(page).toHaveURL(/\/app$/)
  await expect(page.getByRole('heading', { name: /Здравствуйте/ })).toBeVisible()
})

test('resident dashboard renders KPI cards, charts, payments, and announcements', async ({ page }) => {
  await loginResident(page, updatedResidentPassword)
  await expect(page.getByText('К оплате за этот месяц')).toBeVisible()
  await expect(page.getByText('Баланс', { exact: true })).toBeVisible()
  await expect(page.getByText('Общий долг')).toBeVisible()
  await expect(page.getByText('Ближайший срок оплаты')).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Начислено и оплачено по месяцам' })).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Структура начислений' })).toBeVisible()
  await expect(page.locator('.recharts-wrapper svg')).toHaveCount(2)
  await expect(page.getByRole('heading', { name: 'Последние платежи' })).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Объявления' }).first()).toBeVisible()
  await expect(page.getByText('E2E: Плановое объявление')).toBeVisible()
})

test('charges, payments, room, announcements, and profile pages render correctly', async ({ page }) => {
  await loginResident(page, updatedResidentPassword)
  const routes = [
    ['/app/charges', 'Начисления'],
    ['/app/payments', 'История платежей'],
    ['/app/room', 'Моя комната'],
    ['/app/announcements', 'Объявления'],
    ['/app/profile', 'Профиль'],
  ] as const

  for (const [route, title] of routes) {
    await page.goto(route)
    await expect(page.getByRole('heading', { name: title }).first()).toBeVisible()
    await expect(page.getByRole('alert')).toHaveCount(0)
    const content = await page.locator('main').innerText()
    expect(content.trim().length, `${route} should render visible content`).toBeGreaterThan(10)
  }
  await page.goto('/app/room')
  await expect(page.getByText('BehaAtel E2E Building')).toBeVisible()
  await expect(page.getByText('E2E-01')).toBeVisible()
  await page.goto('/app/announcements')
  await expect(page.getByText('E2E: Плановое объявление')).toBeVisible()
  await page.goto('/app/profile')
  await expect(page.getByText(residentEmail)).toBeVisible()
})

test('charge and payment filters and pagination work', async ({ page }) => {
  await loginResident(page, updatedResidentPassword)
  await page.goto('/app/charges')
  await page.getByLabel('Строк на странице').selectOption('2')
  await page.getByRole('button', { name: 'Следующая страница' }).click()
  await expect(page.getByText(/Страница 2 из/)).toBeVisible()
  await page.getByRole('button', { name: 'Предыдущая страница' }).click()
  await page.getByLabel('Месяц').fill(new Date().toISOString().slice(0, 7))
  await page.getByLabel('Статус').selectOption('unpaid')
  await expect(page.getByText(/Страница 1 из/)).toBeVisible()
  await expect(page.locator('tbody tr')).toHaveCount(2)

  await page.goto('/app/payments')
  await page.getByLabel('Строк на странице').selectOption('2')
  await page.getByRole('button', { name: 'Следующая страница' }).click()
  await expect(page.getByText(/Страница 2 из/)).toBeVisible()
  await page.getByLabel('С даты').fill(new Date(new Date().getFullYear(), new Date().getMonth() - 3, 1).toISOString().slice(0, 10))
  await page.getByLabel('По дату').fill(new Date().toISOString().slice(0, 10))
  await expect(page.getByText(/Страница 1 из/)).toBeVisible()
  await expect(page.getByRole('button', { name: 'Предыдущая страница' })).toBeDisabled()
})

test('dark theme persists, logout clears session, and resident is denied admin access', async ({ page }) => {
  await loginResident(page, updatedResidentPassword)
  await page.getByLabel('Выберите тему оформления').selectOption('dark')
  await expect.poll(() => page.evaluate(() => document.documentElement.classList.contains('dark'))).toBe(true)
  await page.reload()
  await expect.poll(() => page.evaluate(() => document.documentElement.classList.contains('dark'))).toBe(true)
  await page.getByRole('button', { name: 'Выйти' }).click()
  await expect(page).toHaveURL(/\/login$/)
  await page.goto('/app')
  await expect(page).toHaveURL(/\/login$/)

  await loginResident(page, updatedResidentPassword)
  await page.goto('/admin')
  await expect(page).toHaveURL(/\/403$/)
  await expect(page.getByRole('heading', { name: 'Нет доступа' })).toBeVisible()
})

test('SuperAdmin can open every administration section', async ({ page }) => {
  await page.goto('/login')
  await page.getByLabel('Электронная почта').fill(adminEmail)
  await page.getByLabel('Пароль').fill(adminPassword)
  await page.getByRole('button', { name: 'Войти' }).click()
  await expect(page).toHaveURL(/\/admin$/)
  await expect(page.locator('aside nav')).toBeVisible()

  const sections = [
    ['Дашборд', '/admin'],
    ['Жильцы', '/admin/residents'],
    ['Заявки на регистрацию', '/admin/registrations'],
    ['Здания и комнаты', '/admin/rooms'],
    ['Начисления', '/admin/charges'],
    ['Платежи', '/admin/payments'],
    ['Должники', '/admin/debtors'],
    ['Объявления', '/admin/announcements'],
    ['Администраторы', '/admin/admins'],
    ['Журнал действий', '/admin/audit'],
    ['Настройки', '/admin/settings'],
  ] as const
  for (const [label, route] of sections) {
    await page.goto(route)
    await expect(page.getByRole('heading', { name: label }).first()).toBeVisible()
  }
})

test('desktop and mobile screenshots capture light and dark resident pages', async ({ page }) => {
  await mkdir(screenshotsDirectory, { recursive: true })
  await loginResident(page, updatedResidentPassword)
  const targets = [
    ['/app', 'home'],
    ['/app/charges', 'charges'],
    ['/app/payments', 'payments'],
  ] as const
  const themes = ['light', 'dark'] as const
  const viewports = [
    ['desktop', { width: 1440, height: 900 }],
    ['mobile', { width: 390, height: 844 }],
  ] as const

  for (const theme of themes) {
    await page.getByLabel('Выберите тему оформления').selectOption(theme)
    for (const [route, pageName] of targets) {
      for (const [viewportName, viewport] of viewports) {
        await page.setViewportSize(viewport)
        await page.goto(route)
        await page.waitForLoadState('networkidle')
        await expect(page.locator('main')).not.toBeEmpty()
        if (viewportName === 'mobile') {
          await expectNoHorizontalOverflow(page)
        }
        await page.screenshot({
          path: path.join(screenshotsDirectory, `${pageName}-${theme}-${viewportName}.png`),
          fullPage: true,
        })
      }
    }
  }
})

async function expectNoHorizontalOverflow(page: Page): Promise<void> {
  const horizontalOverflow = await page.evaluate(() => (
    document.documentElement.scrollWidth > window.innerWidth
    || document.body.scrollWidth > window.innerWidth
  ))
  expect(horizontalOverflow, 'mobile page must not scroll horizontally').toBe(false)
}

async function loginResident(page: Page, password: string): Promise<void> {
  await page.goto('/login')
  await page.getByLabel('Электронная почта').fill(residentEmail)
  await page.getByLabel('Пароль').fill(password)
  await page.getByRole('button', { name: 'Войти' }).click()
  if (await page.getByRole('heading', { name: 'Обновите пароль' }).isVisible().catch(() => false)) {
    await page.getByLabel('Текущий пароль').fill(password)
    await page.getByLabel('Новый пароль').fill(updatedResidentPassword)
    await page.getByLabel('Повторите новый пароль').fill(updatedResidentPassword)
    await page.getByRole('button', { name: 'Обновить пароль' }).click()
    await expect(page).toHaveURL(/\/login$/)
    await page.getByLabel('Электронная почта').fill(residentEmail)
    await page.getByLabel('Пароль').fill(updatedResidentPassword)
    await page.getByRole('button', { name: 'Войти' }).click()
  }
  await expect(page).toHaveURL(/\/app$/)
}
