'use client';

import { createContext, useContext, useEffect, useState, ReactNode } from 'react';

// User payload kept in context. Mirrors the four keys persisted to localStorage on login.
export interface UserInfo {
  userId: string;
  accessToken: string;
  userName: string;
  role: string;
}

interface UserContextValue {
  user: UserInfo | null;
  // Update both state and localStorage. Call this on successful login.
  setUser: (u: UserInfo) => void;
  // Clear both state and localStorage. Call this on logout.
  clearUser: () => void;
}

const UserContext = createContext<UserContextValue | undefined>(undefined);

// Keys this context owns in localStorage; cross-tab storage events filter against this list.
const STORAGE_KEYS = ['userId', 'accessToken', 'userName', 'role'] as const;

// Read the four keys from localStorage. Returns null if any required key is missing
// or if running in a non-browser environment (SSR).
function readFromStorage(): UserInfo | null {
  if (typeof window === 'undefined') return null;
  const userId = localStorage.getItem('userId');
  const accessToken = localStorage.getItem('accessToken');
  const userName = localStorage.getItem('userName');
  const role = localStorage.getItem('role');
  if (!userId || !accessToken || !userName || !role) return null;
  return { userId, accessToken, userName, role };
}

export function UserProvider({ children }: { children: ReactNode }) {
  // Initial render starts with null. Hydration from localStorage happens in the effect below,
  // so consumers should guard for the null state during the first paint after mount.
  const [user, setUserState] = useState<UserInfo | null>(null);

  // Hydrate from localStorage on first client render.
  useEffect(() => {
    const stored = readFromStorage();
    if (stored) setUserState(stored);
  }, []);

  // Cross-tab sync: when another tab logs in or out, refresh this tab's user state.
  // Note: the storage event does NOT fire in the tab that writes the value, so same-tab
  // login must call setUser() explicitly.
  useEffect(() => {
    const onStorage = (e: StorageEvent) => {
      if (e.key && (STORAGE_KEYS as readonly string[]).includes(e.key)) {
        setUserState(readFromStorage());
      }
    };
    window.addEventListener('storage', onStorage);
    return () => window.removeEventListener('storage', onStorage);
  }, []);

  const setUser = (u: UserInfo) => {
    localStorage.setItem('userId', u.userId);
    localStorage.setItem('accessToken', u.accessToken);
    localStorage.setItem('userName', u.userName);
    localStorage.setItem('role', u.role);
    setUserState(u);
  };

  const clearUser = () => {
    STORAGE_KEYS.forEach(k => localStorage.removeItem(k));
    setUserState(null);
  };

  return (
    <UserContext.Provider value={{ user, setUser, clearUser }}>
      {children}
    </UserContext.Provider>
  );
}

// Hook accessor. Throws when used outside <UserProvider> to surface misuse early.
export function useUser(): UserContextValue {
  const ctx = useContext(UserContext);
  if (!ctx) throw new Error('useUser must be used inside <UserProvider>');
  return ctx;
}
