import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'

import { renderWithProviders } from '../test/render-utils'
import HomePage from './HomePage'

describe('<HomePage />', () => {
  it('toggles both menu and chat drawers', async () => {
    const user = userEvent.setup()

    renderWithProviders(<HomePage />, { route: '/', isAuthenticated: true })

    const collapseMenuButton = screen.getByRole('button', {
      name: /collapse menu/i,
    })
    const collapseChatButton = screen.getByRole('button', {
      name: /collapse chat drawer/i,
    })

    await user.click(collapseMenuButton)
    await user.click(collapseChatButton)

    expect(
      screen.getByRole('button', { name: /expand menu/i }),
    ).toBeInTheDocument()
    expect(
      screen.getByRole('button', { name: /expand chat drawer/i }),
    ).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: /expand menu/i }))
    await user.click(
      screen.getByRole('button', { name: /expand chat drawer/i }),
    )

    expect(
      screen.getByRole('button', { name: /collapse menu/i }),
    ).toBeInTheDocument()
    expect(
      screen.getByRole('button', { name: /collapse chat drawer/i }),
    ).toBeInTheDocument()
  })
})
