import type { ComponentProps } from 'react'
import { Toaster as SonnerToaster } from 'sonner'

export function Toaster(props: ComponentProps<typeof SonnerToaster>) {
  return <SonnerToaster {...props} />
}
