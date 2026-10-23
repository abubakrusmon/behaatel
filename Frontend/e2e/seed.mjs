import { randomInt } from 'node:crypto'
import { mkdir, readFile, writeFile } from 'node:fs/promises'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const baseUrl = 'http://localhost:5000/api/v1'
const adminEmail = process.env.E2E_ADMIN_EMAIL ?? 'superadmin@behaatel.com'
const adminPassword = process.env.E2E_ADMIN_PASSWORD
const residentEmail = 'e2e-resident@behaatel.test'
const projectDirectory = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..')
const credentialsPath = path.join(path.dirname(fileURLToPath(import.meta.url)), '.e2e-credentials.json')
let accessToken

if (!adminPassword) {
  console.error('Missing E2E_ADMIN_PASSWORD environment variable.')
  process.exit(1)
}

function log(message) {
  console.log(`[e2e:seed] ${message}`)
}

function randomPassword() {
  const groups = [
    'ABCDEFGHJKLMNPQRSTUVWXYZ',
    'abcdefghijkmnopqrstuvwxyz',
    '23456789',
    '!@#$%&*_-+',
  ]
  const allCharacters = groups.join('')
  const characters = groups.map((group) => group[randomInt(group.length)])
  while (characters.length < 28) characters.push(allCharacters[randomInt(allCharacters.length)])
  for (let index = characters.length - 1; index > 0; index--) {
    const swapIndex = randomInt(index + 1)
    ;[characters[index], characters[swapIndex]] = [characters[swapIndex], characters[index]]
  }
  return characters.join('')
}

async function request(method, endpoint, body, { login = false } = {}) {
  const headers = { Accept: 'application/json' }
  if (body !== undefined) headers['Content-Type'] = 'application/json'
  if (!login && accessToken) headers.Authorization = `Bearer ${accessToken}`

  const attempts = login ? 4 : 1
  for (let attempt = 1; attempt <= attempts; attempt++) {
    let response
    try {
      response = await fetch(`${baseUrl}${endpoint}`, {
        method,
        headers,
        body: body === undefined ? undefined : JSON.stringify(body),
        signal: AbortSignal.timeout(30_000),
      })
    } catch (error) {
      if (login && attempt < attempts) {
        log(`${method} ${endpoint}: network error (${attempt}/${attempts - 1} retries used); retrying in 3 seconds.`)
        await new Promise((resolve) => setTimeout(resolve, 3000))
        continue
      }
      throw new Error(`${method} ${endpoint} failed with a network error after ${attempt} attempt(s): ${error.name}`)
    }

    log(`${method} ${endpoint} -> HTTP ${response.status} ${response.statusText}`)
    if (login && response.status === 401) {
      throw new Error('SuperAdmin sign-in returned HTTP 401. Stop: check credentials and lockout policy.')
    }
    if (!response.ok) {
      throw new Error(`${method} ${endpoint} returned HTTP ${response.status}.`)
    }

    if (response.status === 204) return null
    const responseText = await response.text()
    return responseText ? JSON.parse(responseText) : null
  }
}

async function getOrCreateBuilding() {
  const buildings = await request('GET', '/admin/buildings')
  const existing = buildings.find((building) => building.name === 'BehaAtel E2E Building')
  if (existing) {
    log(`Building already exists: ${existing.name}.`)
    return existing
  }
  const created = await request('POST', '/admin/buildings', {
    name: 'BehaAtel E2E Building',
    address: 'E2E test address',
    floorsCount: 2,
  })
  log(`Building created: ${created.name}.`)
  return created
}

async function getOrCreateRoom(building) {
  const roomsPage = await request(
    'GET',
    `/admin/rooms?page=1&pageSize=100&buildingId=${encodeURIComponent(building.id)}`,
  )
  const existing = roomsPage.items.find((room) => room.number === 'E2E-01')
  if (existing) {
    log(`Room already exists: ${existing.number}; normalizing capacity and bed price.`)
    await request('PUT', `/admin/rooms/${existing.id}`, {
      number: existing.number,
      floor: 1,
      capacity: 2,
      pricePerBed: 300,
      type: 'Standard',
    })
    return { ...existing, capacity: 2, pricePerBed: 300 }
  }

  const created = await request('POST', '/admin/rooms', {
    buildingId: building.id,
    number: 'E2E-01',
    floor: 1,
    capacity: 2,
    pricePerBed: 300,
    type: 'Standard',
  })
  log(`Room created: ${created.number}.`)
  return created
}

async function getOrCreateResident(room, password) {
  const residentsPage = await request(
    'GET',
    `/admin/residents?page=1&pageSize=100&search=${encodeURIComponent(residentEmail)}`,
  )
  let resident = residentsPage.items.find((item) => item.email.toLowerCase() === residentEmail)
  if (!resident) {
    resident = await request('POST', '/admin/residents', {
      email: residentEmail,
      fullName: 'E2E Test Resident',
      phone: null,
      password,
      passportNumber: 'E2E-RESIDENT-001',
      birthDate: '1995-01-15',
      organization: 'E2E test account',
      roomId: room.id,
      checkInDate: dateOnly(new Date()),
    })
    log(`Resident created and assigned to ${room.number}.`)
    return resident
  }

  log(`Resident already exists: ${residentEmail}; resetting its test password.`)
  await request('POST', `/admin/residents/${resident.id}/reset-password`, { newPassword: password })
  if (!resident.isActive) {
    await request('POST', `/admin/residents/${resident.id}/unblock`)
    log('Existing resident unblocked.')
  }
  if (resident.buildingName !== 'BehaAtel E2E Building' || resident.roomNumber !== room.number) {
    if (resident.status === 'active') {
      await request('POST', `/admin/residents/${resident.id}/check-out`, { date: dateOnly(new Date()) })
    }
    await request('POST', `/admin/residents/${resident.id}/check-in`, {
      roomId: room.id,
      date: dateOnly(new Date()),
    })
    log(`Existing resident assigned to ${room.number}.`)
  } else {
    log(`Existing resident already assigned to ${room.number}.`)
  }
  return resident
}

function dateOnly(date) {
  return date.toISOString().slice(0, 10)
}

async function ensureCharges(residentId) {
  const page = await request('GET', `/admin/charges?page=1&pageSize=100&residentId=${encodeURIComponent(residentId)}`)
  const fixtures = [
    { type: 'rent', amount: 300 },
    { type: 'utilities', amount: 45 },
    { type: 'other', amount: 15 },
  ]
  const now = new Date()
  let createdCount = 0
  let existingCount = 0

  for (let offset = 3; offset >= 0; offset--) {
    const month = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth() - offset, 1))
    const period = dateOnly(month)
    const dueDate = dateOnly(new Date(Date.UTC(month.getUTCFullYear(), month.getUTCMonth(), 10)))
    for (const fixture of fixtures) {
      const exists = page.items.some((charge) =>
        charge.period === period && charge.type === fixture.type && charge.description === `E2E ${fixture.type} ${period.slice(0, 7)}`,
      )
      if (exists) {
        existingCount++
        continue
      }
      await request('POST', '/admin/charges', {
        residentId,
        period,
        type: fixture.type,
        amount: fixture.amount,
        dueDate,
        description: `E2E ${fixture.type} ${period.slice(0, 7)}`,
      })
      createdCount++
    }
  }
  log(`Charges: created ${createdCount}, already existed ${existingCount}; four periods, three types each.`)
}

async function ensurePayments(residentId) {
  const page = await request('GET', `/admin/payments?page=1&pageSize=100&residentId=${encodeURIComponent(residentId)}`)
  const fixtures = [
    { method: 'Cash', amount: 60, comment: 'E2E fixture payment cash', daysAgo: 1 },
    { method: 'Card', amount: 75, comment: 'E2E fixture payment card', daysAgo: 3 },
    { method: 'BankTransfer', amount: 90, comment: 'E2E fixture payment bank transfer', daysAgo: 5 },
  ]
  let createdCount = 0
  let existingCount = 0

  for (const fixture of fixtures) {
    if (page.items.some((payment) => payment.comment === fixture.comment)) {
      existingCount++
      log(`Payment already exists: ${fixture.method}.`)
      continue
    }
    await request('POST', '/admin/payments', {
      residentId,
      amount: fixture.amount,
      method: fixture.method,
      paidAt: new Date(Date.now() - fixture.daysAgo * 86_400_000).toISOString(),
      comment: fixture.comment,
    })
    createdCount++
  }
  log(`Payments: created ${createdCount}, already existed ${existingCount}.`)
}

async function ensureAnnouncements() {
  const page = await request('GET', '/admin/announcements?page=1&pageSize=100')
  const fixtures = [
    {
      title: 'E2E: Плановое объявление',
      body: 'Опубликованное тестовое объявление для проверки кабинета жильца.',
      publishNow: true,
    },
    {
      title: 'E2E: Черновик объявления',
      body: 'Черновик для проверки списка объявлений в панели администратора.',
      publishNow: false,
    },
  ]
  for (const fixture of fixtures) {
    if (page.items.some((announcement) => announcement.title === fixture.title)) {
      log(`Announcement already exists: ${fixture.title}.`)
      continue
    }
    await request('POST', '/admin/announcements', fixture)
    log(`Announcement created: ${fixture.title} (${fixture.publishNow ? 'published' : 'draft'}).`)
  }
}

async function main() {
  log(`Starting API seed at ${baseUrl}; SuperAdmin email: ${adminEmail}.`)
  const loginResult = await request(
    'POST',
    '/auth/login',
    { email: adminEmail, password: adminPassword },
    { login: true },
  )
  if (loginResult.requiresCode || !loginResult.session?.accessToken) {
    throw new Error('SuperAdmin login did not return a usable session. Check whether email 2FA is enabled.')
  }
  if (loginResult.session.user?.mustChangePassword) {
    throw new Error('SuperAdmin must change password before seeding. No other login attempt was made.')
  }
  accessToken = loginResult.session.accessToken
  log('SuperAdmin login succeeded; access token remains in memory only.')

  const previousCredentials = await readFile(credentialsPath, 'utf8')
    .then((contents) => JSON.parse(contents))
    .catch(() => null)
  const residentPassword = randomPassword()
  const building = await getOrCreateBuilding()
  const room = await getOrCreateRoom(building)
  const resident = await getOrCreateResident(room, residentPassword)
  await ensureCharges(resident.id)
  await ensurePayments(resident.id)
  await ensureAnnouncements()

  const credentials = {
    email: residentEmail,
    password: residentPassword,
    newPassword: residentPassword,
  }
  await mkdir(path.dirname(credentialsPath), { recursive: true })
  await writeFile(credentialsPath, `${JSON.stringify(credentials, null, 2)}\n`, { mode: 0o600 })
  log(`Resident credentials saved to ${path.relative(projectDirectory, credentialsPath)}; previous credential file ${previousCredentials ? 'was replaced' : 'did not exist'}.`)
  console.log(`E2E resident account (save this output securely):\nEmail: ${residentEmail}\nPassword: ${residentPassword}`)
  log('Seed completed successfully. No token was written or printed.')
}

main().catch((error) => {
  console.error(`[e2e:seed] FAILED: ${error.message}`)
  process.exitCode = 1
})
