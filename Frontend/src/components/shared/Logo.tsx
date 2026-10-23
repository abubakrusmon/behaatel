import { Building2 } from 'lucide-react'

export function Logo({ light = false, showText = true }: { light?: boolean; showText?: boolean }) {
  return (
    <span className={`inline-flex items-center ${showText ? 'gap-2.5' : ''} font-semibold tracking-tight ${light ? 'text-white' : 'text-foreground'}`}>
      <span className="grid size-10 place-items-center rounded-xl bg-primary text-white shadow-sm">
        <Building2 aria-hidden="true" size={21} strokeWidth={2.2} />
      </span>
      {showText && <span className="text-lg">BehaAtel</span>}
    </span>
  )
}
