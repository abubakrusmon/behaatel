import type { PropsWithChildren, HTMLAttributes, LabelHTMLAttributes } from 'react'
import { FormProvider, useFormContext, type FieldValues, type Path } from 'react-hook-form'
import { Label } from '@/components/ui/label'
import { cn } from '@/lib/utils'

export { FormProvider as Form }

export function FormItem({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('space-y-2', className)} {...props} />
}

export function FormLabel({ className, ...props }: LabelHTMLAttributes<HTMLLabelElement>) {
  return <Label className={className} {...props} />
}

export function FormDescription({ className, ...props }: HTMLAttributes<HTMLParagraphElement>) {
  return <p className={cn('text-sm text-muted-foreground', className)} {...props} />
}

export function FormMessage<T extends FieldValues>({ name }: PropsWithChildren<{ name: Path<T> }>) {
  const { formState: { errors } } = useFormContext<T>()
  const message = name.split('.').reduce<unknown>((current, part) => {
    if (!current || typeof current !== 'object') return undefined
    return (current as Record<string, unknown>)[part]
  }, errors)
  if (!message || typeof message !== 'object' || !('message' in message)) return null
  return <p role="alert" className="text-sm text-destructive">{String(message.message)}</p>
}

export function FormControl({ children }: PropsWithChildren) {
  return <>{children}</>
}
