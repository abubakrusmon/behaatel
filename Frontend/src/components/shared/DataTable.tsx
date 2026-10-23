import {
  getCoreRowModel,
  getSortedRowModel,
  useLegacyTable,
  type LegacyColumnDef as ColumnDef,
} from '@tanstack/react-table/legacy'
import { flexRender } from '@tanstack/react-table'
import type { RowData, SortingState } from '@tanstack/table-core'
import { useState } from 'react'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { t } from '@/i18n/ru'

interface DataTableProps<TData extends RowData> {
  columns: ColumnDef<TData, unknown>[]
  data: TData[]
  page: number
  totalPages: number
  onPageChange: (page: number) => void
  onSortChange?: (sort: SortingState) => void
  sorting?: SortingState
  loading?: boolean
  getRowClassName?: (row: TData) => string
}

export function DataTable<TData extends RowData>({
  columns,
  data,
  page,
  totalPages,
  onPageChange,
  onSortChange,
  sorting: controlledSorting,
  loading = false,
  getRowClassName,
}: DataTableProps<TData>) {
  const [internalSorting, setInternalSorting] = useState<SortingState>([])
  const sorting = controlledSorting ?? internalSorting
  const table = useLegacyTable<TData>({
    data,
    columns,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    manualPagination: true,
    manualSorting: true,
    state: { sorting },
    onSortingChange: (updater) => {
      const nextSorting = typeof updater === 'function' ? updater(sorting) : updater
      if (!controlledSorting) setInternalSorting(nextSorting)
      onSortChange?.(nextSorting)
    },
  })

  return (
    <div className="overflow-hidden rounded-2xl border border-border bg-card">
      <div className="max-w-full overflow-x-auto">
        <table className="w-full min-w-[760px] text-left text-sm">
          <thead className="border-b border-border bg-muted/60 text-xs uppercase tracking-wide text-muted-foreground">
            {table.getHeaderGroups().map((group) => (
              <tr key={group.id}>
                {group.headers.map((header) => (
                  <th key={header.id} className="px-5 py-3.5 font-medium">
                    {header.isPlaceholder ? null : (
                      <button
                        className="text-left"
                        type="button"
                        onClick={header.column.getToggleSortingHandler()}
                        disabled={!header.column.getCanSort()}
                      >
                        {flexRender(header.column.columnDef.header, header.getContext())}
                        {header.column.getIsSorted() === 'asc' ? ' ↑' : header.column.getIsSorted() === 'desc' ? ' ↓' : ''}
                      </button>
                    )}
                  </th>
                ))}
              </tr>
            ))}
          </thead>
          <tbody className="divide-y divide-border">
            {loading ? (
              Array.from({ length: 5 }, (_, rowIndex) => (
                <tr key={rowIndex}>
                  {columns.map((_, cellIndex) => (
                    <td key={cellIndex} className="px-5 py-4">
                      <Skeleton className="h-4 w-3/4" />
                    </td>
                  ))}
                </tr>
              ))
            ) : table.getRowModel().rows.length ? (
              table.getRowModel().rows.map((row) => (
                <tr key={row.id} className={`transition-colors hover:bg-muted/50 ${getRowClassName?.(row.original) ?? ''}`}>
                  {row.getVisibleCells().map((cell) => (
                    <td key={cell.id} className="px-5 py-4">
                      {flexRender(cell.column.columnDef.cell, cell.getContext())}
                    </td>
                  ))}
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan={columns.length} className="px-5 py-14 text-center text-muted-foreground">
                  {t('shared.tableEmpty')}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
      <div className="flex items-center justify-between border-t border-border px-4 py-3 text-sm text-muted-foreground">
        <span>
          {t('common.page')} {page} {t('common.of')} {Math.max(1, totalPages)}
        </span>
        <div className="flex gap-2">
          <Button variant="outline" size="icon" aria-label={t('common.previousPage')} disabled={page <= 1} onClick={() => onPageChange(page - 1)}>
            <ChevronLeft size={17} />
          </Button>
          <Button variant="outline" size="icon" aria-label={t('common.nextPage')} disabled={page >= totalPages} onClick={() => onPageChange(page + 1)}>
            <ChevronRight size={17} />
          </Button>
        </div>
      </div>
    </div>
  )
}
