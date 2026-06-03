import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'

import type {
  CreateUserCommand,
  UpdateUserCommand,
  UserDto,
} from '@/api/generated/model'
import { renderWithProviders } from '@/test/render-utils'
import { server } from '@/test/server'

import AdminPanelPage from './AdminPanelPage'

function useUserHandlers(users: UserDto[], roles = ['User', 'Admin']) {
  server.use(
    http.get('*/api/users/roles', () => HttpResponse.json({ roles })),
    http.get('*/api/users', () => HttpResponse.json({ users })),
    http.post('*/api/users', async ({ request }) => {
      const body = (await request.json()) as CreateUserCommand
      const createdUser = {
        id: `user-${users.length + 1}`,
        userName: body.userName,
        email: body.email,
        role: body.role ?? 'User',
      }

      users.push(createdUser)

      return HttpResponse.json({
        userId: createdUser.id,
        userName: createdUser.userName,
        email: createdUser.email,
        role: createdUser.role,
      })
    }),
    http.put('*/api/users/:userId', async ({ params, request }) => {
      const body = (await request.json()) as UpdateUserCommand
      const userId = String(params.userId)
      const user = users.find((candidate) => candidate.id === userId)

      if (!user) {
        return new HttpResponse(null, { status: 404 })
      }

      user.userName = body.userName ?? user.userName
      user.email = body.email ?? user.email
      user.role = body.role ?? user.role

      return HttpResponse.json({
        userId: user.id,
        userName: user.userName,
        email: user.email,
        role: user.role,
      })
    }),
    http.delete('*/api/users/:userId', ({ params }) => {
      const userId = String(params.userId)
      const userIndex = users.findIndex((candidate) => candidate.id === userId)

      if (userIndex === -1) {
        return new HttpResponse(null, { status: 404 })
      }

      users.splice(userIndex, 1)

      return new HttpResponse(null, { status: 204 })
    }),
  )
}

describe('<AdminPanelPage />', () => {
  it('renders users and creates a new user', async () => {
    const user = userEvent.setup()
    const users: UserDto[] = [
      {
        id: 'user-1',
        userName: 'Ava Stone',
        email: 'ava@example.com',
        role: 'Admin',
      },
    ]
    useUserHandlers(users, ['Analyst', 'Admin'])

    renderWithProviders(<AdminPanelPage />, {
      authClaims: { role: 'Admin' },
      isAuthenticated: true,
    })

    expect(await screen.findByText('ava@example.com')).toBeInTheDocument()

    const addUserForm = screen.getByRole('form', { name: /add new user/i })

    await waitFor(() => {
      expect(within(addUserForm).getByText('Analyst')).toBeInTheDocument()
    })

    await user.type(
      within(addUserForm).getByLabelText(/username/i),
      'Mira Vale',
    )
    await user.type(
      within(addUserForm).getByLabelText(/email/i),
      'mira@example.com',
    )
    await user.click(
      within(addUserForm).getByRole('button', { name: /create user/i }),
    )

    expect(await screen.findByText('mira@example.com')).toBeInTheDocument()
    expect(
      within(screen.getByRole('table', { name: /users/i })).getByText(
        'Analyst',
      ),
    ).toBeInTheDocument()
  })

  it('shows the API validation message when creating a user fails', async () => {
    const user = userEvent.setup()
    const users: UserDto[] = [
      {
        id: 'user-1',
        userName: 'Ava Stone',
        email: 'ava@example.com',
        role: 'Admin',
      },
    ]
    useUserHandlers(users, ['User', 'Admin'])
    server.use(
      http.post('*/api/users', () =>
        HttpResponse.json(
          {
            title: 'One or more validation errors occurred.',
            status: 400,
            errors: {
              UserName: ['A user with this login already exists.'],
            },
          },
          { status: 400 },
        ),
      ),
    )

    renderWithProviders(<AdminPanelPage />, {
      authClaims: { role: 'Admin' },
      isAuthenticated: true,
    })

    const addUserForm = screen.getByRole('form', { name: /add new user/i })

    await waitFor(() => {
      expect(within(addUserForm).getByText('User')).toBeInTheDocument()
    })

    await user.type(
      within(addUserForm).getByLabelText(/username/i),
      'Ava Stone',
    )
    await user.type(
      within(addUserForm).getByLabelText(/email/i),
      'ava@example.com',
    )
    await user.click(
      within(addUserForm).getByRole('button', { name: /create user/i }),
    )

    expect(
      await screen.findByText('A user with this login already exists.'),
    ).toBeInTheDocument()
  })

  it('opens a user from the list and saves edits', async () => {
    const user = userEvent.setup()
    const users: UserDto[] = [
      {
        id: 'user-1',
        userName: 'Ava Stone',
        email: 'ava@example.com',
        role: 'Admin',
      },
    ]
    useUserHandlers(users)

    renderWithProviders(<AdminPanelPage />, {
      authClaims: { role: 'Admin' },
      isAuthenticated: true,
    })

    await user.click(
      await screen.findByRole('button', { name: /edit ava stone/i }),
    )

    const dialog = await screen.findByRole('dialog', { name: /edit user/i })
    const emailInput = within(dialog).getByLabelText(/email/i)

    await user.clear(emailInput)
    await user.type(emailInput, 'ava.updated@example.com')
    await user.click(
      within(dialog).getByRole('button', { name: /save changes/i }),
    )

    expect(
      await screen.findByText('ava.updated@example.com'),
    ).toBeInTheDocument()
  })

  it('deletes a user after confirmation', async () => {
    const user = userEvent.setup()
    const users: UserDto[] = [
      {
        id: 'admin-id',
        userName: 'Ava Stone',
        email: 'ava@example.com',
        role: 'Admin',
      },
      {
        id: 'user-1',
        userName: 'Mira Vale',
        email: 'mira@example.com',
        role: 'User',
      },
    ]
    useUserHandlers(users)

    renderWithProviders(<AdminPanelPage />, {
      authClaims: { role: 'Admin', sub: 'admin-id' },
      isAuthenticated: true,
    })

    expect(await screen.findByText('mira@example.com')).toBeInTheDocument()

    await user.click(
      await screen.findByRole('button', { name: /delete mira vale/i }),
    )

    const dialog = await screen.findByRole('dialog', { name: /delete user/i })
    await user.click(
      within(dialog).getByRole('button', { name: /delete user/i }),
    )

    await waitFor(() => {
      expect(screen.queryByText('mira@example.com')).not.toBeInTheDocument()
    })
  })

  it('disables deleting the current admin user', async () => {
    const users: UserDto[] = [
      {
        id: 'admin-id',
        userName: 'Ava Stone',
        email: 'ava@example.com',
        role: 'Admin',
      },
    ]
    useUserHandlers(users)

    renderWithProviders(<AdminPanelPage />, {
      authClaims: { role: 'Admin', sub: 'admin-id' },
      isAuthenticated: true,
    })

    const deleteButton = await screen.findByRole('button', {
      name: /delete ava stone/i,
    })

    expect(deleteButton).toBeDisabled()
  })

  it('keeps the current session when users endpoint is unauthorized', async () => {
    server.use(
      http.get('*/api/users/roles', () =>
        HttpResponse.json({ roles: ['User', 'Admin'] }),
      ),
      http.get('*/api/users', () => new HttpResponse(null, { status: 401 })),
    )

    renderWithProviders(<AdminPanelPage />, {
      authClaims: { role: 'Admin' },
      isAuthenticated: true,
    })

    expect(await screen.findByText(/unable to load users/i)).toBeInTheDocument()
    expect(localStorage.getItem('auth_token')).not.toBeNull()
  })
})
