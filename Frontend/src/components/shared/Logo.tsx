export function Logo({
  light = false,
  showText = true,
  size = 40,
  layout = 'horizontal',
}: {
  light?: boolean
  showText?: boolean
  size?: 36 | 40 | 56 | 60 | 64 | 72
  layout?: 'horizontal' | 'vertical'
}) {
  return (
    <span className={`inline-flex ${layout === 'vertical' ? 'flex-col' : 'flex-row'} items-center ${showText ? layout === 'vertical' ? 'gap-2' : 'gap-2.5' : ''} font-semibold tracking-tight ${light ? 'text-white' : 'text-foreground'}`}>
      <img
        src="/brand/logo-mark.png"
        alt="BehaAtel"
        width={size}
        height={size}
        className="shrink-0 rounded-xl object-contain"
      />
      {showText && <span className="text-lg">BehaAtel</span>}
    </span>
  )
}
