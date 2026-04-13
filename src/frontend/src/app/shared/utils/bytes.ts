export function formatBytes(value: number | null | undefined): string {
  if (value == null || Number.isNaN(value)) {
    return '0 B';
  }

  if (value < 1024) {
    return `${value} B`;
  }

  const units = ['KB', 'MB', 'GB', 'TB'];
  let size = value;
  let index = -1;

  do {
    size /= 1024;
    index += 1;
  } while (size >= 1024 && index < units.length - 1);

  return `${size.toFixed(size >= 10 ? 0 : 1)} ${units[index]}`;
}
