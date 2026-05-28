import '@testing-library/jest-dom/vitest'

import { cleanup } from '@testing-library/react'
import { toast } from 'sonner'
import { afterAll, afterEach, beforeAll } from 'vitest'

import { server } from './server'

beforeAll(() => server.listen({ onUnhandledRequest: 'error' }))
afterEach(() => {
  cleanup()
  toast.dismiss()
  server.resetHandlers()
  localStorage.clear()
})
afterAll(() => server.close())
