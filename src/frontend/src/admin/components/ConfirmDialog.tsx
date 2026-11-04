import { useTranslation } from 'react-i18next'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'

export interface ConfirmDialogContent {
  title: string
  description: string
  confirmLabel: string
  destructive: boolean
}

interface ConfirmDialogProps {
  content: ConfirmDialogContent | null
  onConfirm: () => void
  onCancel: () => void
}

export function ConfirmDialog({ content, onConfirm, onCancel }: Readonly<ConfirmDialogProps>) {
  const { t } = useTranslation()

  return (
    <AlertDialog
      open={content !== null}
      onOpenChange={(open) => {
        if (!open) {
          onCancel()
        }
      }}
    >
      {content && (
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{content.title}</AlertDialogTitle>
            <AlertDialogDescription>{content.description}</AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>{t('admin.cancel')}</AlertDialogCancel>
            <AlertDialogAction
              variant={content.destructive ? 'destructive' : 'default'}
              onClick={onConfirm}
            >
              {content.confirmLabel}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      )}
    </AlertDialog>
  )
}
