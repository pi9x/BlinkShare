export interface AppRuntimeConfig {
  apiBaseUrl: string;
  appName: string;
  anonymousMaxFileSizeBytes: number;
  freeAccountMaxFileSizeBytes: number;
}

const defaultConfig: AppRuntimeConfig = {
  apiBaseUrl: '',
  appName: 'BlinkShare',
  anonymousMaxFileSizeBytes: 512 * 1024,
  freeAccountMaxFileSizeBytes: 1024 * 1024,
};

const runtimeConfig = (
  globalThis as typeof globalThis & {
    __BLINKSHARE_RUNTIME_CONFIG__?: Partial<AppRuntimeConfig>;
  }
).__BLINKSHARE_RUNTIME_CONFIG__;

export const APP_RUNTIME_CONFIG: AppRuntimeConfig = {
  ...defaultConfig,
  ...runtimeConfig,
};
