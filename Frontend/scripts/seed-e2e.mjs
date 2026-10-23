import { randomBytes } from 'node:crypto'

const apiUrl = process.env.VITE_API_URL ?? 'http://localhost:5000/api/v1'
const email = 'e2e-resident@behaatel.test'
const superAdminEmail = process.env.E2E_SUPERADMIN_EMAIL
const superAdminPassword = process.env.E2E_SUPERADMIN_PASSWORD

if (!superAdminEmail || !superAdminPassword) {
  throw new Error('SeedAdmin credentials were not provided through the process environment.')
}

function strongPassword() {
  return `E2e-${randomBytes(15).toString('base64url')}!7a`
}

async function request(path, { token, method = 'GET', body, allowNotFound = false } = {}) {
  const response = await fetch(`${apiUrl}${path}`, {
    method,
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(body ? { 'Content-Type': 'application/json' } : {}),
    },
    ...(body ? { body: JSON.stringify(body) } : {}),
  })

  if (allowNotFound && response.status === 404) return null
  const text = await response.text()
  if (!response.ok) {
    let detail = text
    try {
      const problem = JSON.parse(text)
      detail = problem.detail ?? problem.title ?? text
    } catch {
      detail = text
    }
    throw new Error(`${method} ${path} returned ${response.status}: ${detail}`)
  }
  return text ? JSON.parse(text) : null
}

const login = await request('/auth/login', {
  method: 'POST',
  body: { email: superAdminEmail, password: superAdminPassword },
})
if (!login?.accessToken) {
  throw new Error('SuperAdmin login did not return an access token. Check the configured SeedAdmin account.')
}
const token = login.accessToken
const today = new Date()
const todayIso = today.toISOString().slice(0, 10)

const buildings = await request('/admin/buildings', { token })
let building = buildings.find((item) => item.name === 'E2E BehaAtel Building')
if (!building) {
  building = await request('/admin/buildings', {
    token,
    method: 'POST',
    body: { name: 'E2E BehaAtel Building', address: 'E2E Test Avenue 1', floorsCount: 5 },
  })
}

const roomsPage = await request('/admin/rooms?page=1&pageSize=100&search=E2E-01', { token })
let room = roomsPage.items.find((item) => item.buildingId === building.id && item.number === 'E2E-01')
if (!room) {
  room = await request('/admin/rooms', {
    token,
    method: 'POST',
    body: { buildingId: building.id, number: 'E2E-01', floor: 1, capacity: 4, pricePerBed: 850, type: 'standard' },
  })
}

const residentPage = await request(`/admin/residents?page=1&pageSize=20&search=${encodeURIComponent(email)}`, { token })
let resident = residentPage.items.find((item) => item.email.toLowerCase() === email)
let password = strongPassword()
if (!resident) {
  resident = await request('/admin/residents', {
    token,
    method: 'POST',
    body: {
      email,
      fullName: 'Тестовый жилец E2E',
      phone: '+992900000001',
      password,
      passportNumber: `E2E-${randomBytes(8).toString('hex')}`,
      birthDate: '1995-01-01',
      organization: 'BehaAtel E2E',
      roomId: room.id,
      checkInDate: todayIso,
    },
  })
} else {
  await request(`/admin/residents/${resident.id}/reset-password`, {
    token,
    method: 'POST',
    body: { newPassword: password },
  })
}

const chargesPage = await request(`/admin/charges?page=1&pageSize=100&residentId=${resident.id}`, { token })
const periods = Array.from({ length: 4 }, (_, index) => {
  const date = new Date(today.getFullYear(), today.getMonth() - index, 1)
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-01`
})
for (const period of periods) {
  const [year, month] = period.split('-').map(Number)
  for (const [type, amount] of [['rent', 850], ['utilities', 120], ['other', 35]]) {
    if (chargesPage.items.some((charge) => charge.period.slice(0, 10) === period
      && charge.type === type && charge.status !== 'cancelled')) continue
    await request('/admin/charges', {
      token,
      method: 'POST',
      body: {
        residentId: resident.id,
        period,
        type,
        amount,
        dueDate: `${period.slice(0, 7)}-${String(Math.min(10, new Date(year, month, 0).getDate())).padStart(2, '0')}`,
        description: `E2E seed ${type} ${period.slice(0, 7)}`,
      },
    })
  }
}

const paymentsPage = await request(`/admin/payments?page=1&pageSize=100&residentId=${resident.id}`, { token })
for (const [index, method] of ['cash', 'card', 'bankTransfer'].entries()) {
  const marker = `E2E seed ${method}`
  const existing = paymentsPage.items.find((payment) => payment.comment === marker)
  if (existing) {
    if (method === 'bankTransfer' && existing.status !== 'cancelled') {
      await request(`/admin/payments/${existing.id}/cancel`, {
        token,
        method: 'POST',
        body: { reason: 'E2E cancelled payment sample' },
      })
    }
    continue
  }
  const paidAt = new Date(today.getTime())
  paidAt.setUTCMonth(paidAt.getUTCMonth() - index)
  const payment = await request('/admin/payments', {
    token,
    method: 'POST',
    body: { residentId: resident.id, amount: 175 + index * 25, method, paidAt: paidAt.toISOString(), comment: marker },
  })
  if (method === 'bankTransfer') {
    await request(`/admin/payments/${payment.id}/cancel`, {
      token,
      method: 'POST',
      body: { reason: 'E2E cancelled payment sample' },
    })
  }
}

const announcementPage = await request('/admin/announcements?page=1&pageSize=100', { token })
for (const [title, publishNow] of [
  ['E2E: важное объявление', true],
  ['E2E: черновик объявления', false],
]) {
  if (announcementPage.items.some((announcement) => announcement.title === title)) continue
  await request('/admin/announcements', {
    token,
    method: 'POST',
    body: { title, body: 'Тестовое объявление для проверки кабинета жильца.', publishNow },
  })
}

console.log(JSON.stringify({ email, password }))
