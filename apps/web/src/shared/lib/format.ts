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
  unpaid: 'ยังไม่ชำระ',
  paid: 'ชำระแล้ว',
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
  paid: 'bg-green-50 text-green-600 border-green-200',
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
