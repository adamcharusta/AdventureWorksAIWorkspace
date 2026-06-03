import { useState } from 'react'

export function useHomeDrawers() {
  const [isChatDrawerOpen, setIsChatDrawerOpen] = useState(true)
  const [isMenuOpen, setIsMenuOpen] = useState(true)

  return {
    chat: {
      onToggle: () => setIsChatDrawerOpen((current) => !current),
      open: isChatDrawerOpen,
    },
    menu: {
      onToggle: () => setIsMenuOpen((current) => !current),
      open: isMenuOpen,
    },
  }
}
