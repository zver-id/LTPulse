import {useState} from 'react'

/**
 * Хук для хранения значения в localStorage.
 * Если localStorage пуст — используется initialValue.
 * При записи игнорирует null/undefined, чтобы не засорять localStorage.
 */
export function useLocalStorageState<T extends string | number>(
  key: string,
  initialValue: T
): [T, (value: T | null | undefined) => void] {
  const [storedValue, setStoredValue] = useState<T>(() => {
    try {
      const item = localStorage.getItem(key);
      return item ? (JSON.parse(item) as T) : initialValue;
    } catch {
      return initialValue;
    }
  });

  const setValue = (value: T | null | undefined) => {
    if (value === null || value === undefined) {
      console.warn(`useLocalStorageState: попытка сохранить undefined/null для ключа "${key}"`, value);
      return;
    }
    try {
      setStoredValue(value);
      localStorage.setItem(key, JSON.stringify(value));
    } catch (error) {
      console.log(error);
    }
  };

  return [storedValue, setValue];
}
