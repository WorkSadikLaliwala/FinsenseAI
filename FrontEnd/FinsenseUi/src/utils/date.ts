// Format ISO date string to readable format
export function formatDate(dateStr: string): string {
  if (!dateStr) return '-'
  try {
    return new Date(dateStr).toLocaleDateString('en-IN', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    })
  } catch {
    return dateStr
  }
}

// Get today's date as ISO string for API requests
export function getTodayISO(): string {
  return new Date().toISOString().split('T')[0]
}
