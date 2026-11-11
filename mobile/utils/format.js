export function formatNumber (value, digits = 1) {
  const number = Number(value || 0)
  return Number.isFinite(number) ? number.toFixed(digits) : '0'
}

export function truncate (text, max = 28) {
  const value = String(text || '')
  return value.length > max ? `${value.slice(0, max)}…` : value
}

export function nowDate () {
  const date = new Date()
  const p = value => String(value).padStart(2, '0')
  return `${date.getFullYear()}-${p(date.getMonth() + 1)}-${p(date.getDate())}`
}
