// Format number as Indian Rupee
export function formatRupee(amount: number): string {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    maximumFractionDigits: 0
  }).format(amount)
}

// Format compact (1,00,000 -> Rs.1.0L)
export function formatRupeeCompact(amount: number): string {
  const isNegative = amount < 0
  const absAmount = Math.abs(amount)

  let formatted = ''
  if (absAmount >= 10000000) {
    formatted = `Rs.${(absAmount / 10000000).toFixed(1)}Cr`
  } else if (absAmount >= 100000) {
    formatted = `Rs.${(absAmount / 100000).toFixed(1)}L`
  } else if (absAmount >= 1000) {
    formatted = `Rs.${(absAmount / 1000).toFixed(1)}K`
  } else {
    formatted = formatRupee(absAmount)
  }

  return isNegative ? `-${formatted}` : formatted
}
