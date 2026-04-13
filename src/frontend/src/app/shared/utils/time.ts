export function formatDateTime(value: string | null | undefined): string {
  if (!value) {
    return 'Unknown';
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value));
}

export function formatRelativeTime(value: string | null | undefined): string {
  if (!value) {
    return 'Unknown';
  }

  const target = new Date(value).getTime();
  const deltaMinutes = Math.round((target - Date.now()) / 60000);

  if (Math.abs(deltaMinutes) < 1) {
    return 'now';
  }

  const formatter = new Intl.RelativeTimeFormat(undefined, { numeric: 'auto' });

  if (Math.abs(deltaMinutes) < 60) {
    return formatter.format(deltaMinutes, 'minute');
  }

  const deltaHours = Math.round(deltaMinutes / 60);
  if (Math.abs(deltaHours) < 24) {
    return formatter.format(deltaHours, 'hour');
  }

  const deltaDays = Math.round(deltaHours / 24);
  return formatter.format(deltaDays, 'day');
}
