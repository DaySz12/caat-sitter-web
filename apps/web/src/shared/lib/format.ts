export function formatCurrency(amount: number): string {
  return `฿${amount.toLocaleString('th-TH', { maximumFractionDigits: 0 })}`;
}

export function formatPerDay(amount: number): string {
  return `${formatCurrency(amount)}/วัน`;
}

export function formatDate(iso: string): string {
  const d = new Date(iso);
  return d.toLocaleDateString('th-TH', { day: 'numeric', month: 'short', year: 'numeric' });
}

export function formatDateRange(startIso: string, endIso: string): string {
  return `${formatDate(startIso)} - ${formatDate(endIso)}`;
}

const STATUS_LABELS_TH: Record<string, string> = {
  pending: 'รอดำเนินการ',
  confirmed: 'ยืนยันแล้ว',
  in_progress: 'กำลังดำเนินการ',
  completed: 'เสร็จสิ้น',
  cancelled: 'ยกเลิกแล้ว',
  rejected: 'ปฏิเสธ',
  unpaid: 'ยังไม่ชำระเงิน',
  pending_verification: 'รอตรวจสอบสลิป',
  paid: 'ชำระเงินแล้ว',
  refunded: 'คืนเงินแล้ว',
};

const STATUS_COLORS: Record<string, string> = {
  pending: 'bg-amber-50 text-amber-600 border-amber-200',
  confirmed: 'bg-blue-50 text-blue-600 border-blue-200',
  in_progress: 'bg-blue-50 text-blue-600 border-blue-200',
  completed: 'bg-green-50 text-green-600 border-green-200',
  cancelled: 'bg-red-50 text-red-600 border-red-200',
  rejected: 'bg-red-50 text-red-600 border-red-200',
  unpaid: 'bg-amber-50 text-amber-600 border-amber-200',
  pending_verification: 'bg-amber-50 text-amber-700 border-amber-300',
  paid: 'bg-emerald-50 text-emerald-600 border-emerald-200',
  refunded: 'bg-slate-100 text-slate-600 border-slate-200',
};

export function statusLabel(status: string): string {
  return STATUS_LABELS_TH[status] ?? status;
}

export function statusColorClass(status: string): string {
  return STATUS_COLORS[status] ?? 'bg-slate-100 text-slate-600 border-slate-200';
}

export function ageDisplay(years: number, months: number): string {
  if (years > 0 && months > 0) return `${years} ปี ${months} เดือน`;
  if (years > 0) return `${years} ปี`;
  return `${months} เดือน`;
}

export function formatTime(iso: string): string {
  return new Date(iso).toLocaleTimeString('th-TH', { hour: '2-digit', minute: '2-digit' });
}

export function formatRelativeTime(iso: string): string {
  const minutes = Math.round((Date.now() - new Date(iso).getTime()) / 60000);
  if (minutes < 1) return 'เมื่อสักครู่';
  if (minutes < 60) return `${minutes} นาทีที่แล้ว`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours} ชั่วโมงที่แล้ว`;
  const days = Math.floor(hours / 24);
  if (days < 7) return `${days} วันที่แล้ว`;
  return formatDate(iso);
}

/** Local calendar date as yyyy-MM-dd, the same shape the API uses for DateOnly values. */
export function toIsoDate(date: Date | string): string {
  return new Date(date).toLocaleDateString('sv-SE');
}
