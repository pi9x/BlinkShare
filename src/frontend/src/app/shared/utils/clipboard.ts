export async function copyText(value: string): Promise<boolean> {
  if (!navigator.clipboard?.writeText) {
    return false;
  }

  try {
    await navigator.clipboard.writeText(value);
    return true;
  } catch {
    return false;
  }
}

export async function readClipboardText(): Promise<string> {
  if (!navigator.clipboard?.readText) {
    return '';
  }

  try {
    return await navigator.clipboard.readText();
  } catch {
    return '';
  }
}
