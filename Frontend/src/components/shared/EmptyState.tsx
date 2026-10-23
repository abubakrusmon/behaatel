import { motion } from 'framer-motion'
import { Blocks } from 'lucide-react'
import { t } from '@/i18n/ru'

export function EmptyState({ title = t('shared.emptyTitle'), description = t('shared.emptyDescription'), badge = true }: { title?: string; description?: string; badge?: boolean }) {
  return (
    <motion.div initial={{ opacity: 0, y: 8 }} animate={{ opacity: 1, y: 0 }} className="flex min-h-72 flex-col items-center justify-center rounded-2xl border border-dashed border-border bg-card/70 px-6 py-12 text-center">
      <div className="mb-5 grid size-14 place-items-center rounded-2xl bg-accent text-primary"><Blocks aria-hidden="true" size={25} /></div>
      <h2 className="text-lg font-semibold">{title}</h2>
      <p className="mt-2 max-w-md text-sm leading-6 text-muted-foreground">{description}</p>
      {badge && <span className="mt-5 rounded-full bg-secondary px-3 py-1 text-xs font-medium text-secondary-foreground">{t('common.inDevelopment')}</span>}
    </motion.div>
  )
}
