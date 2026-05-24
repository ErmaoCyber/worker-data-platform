'use client';

import { createContext, useCallback, useContext, useState } from 'react';

interface NotificationRefreshValue {
  refreshKey: number;
  bumpRefresh: () => void;
}

const NotificationRefreshContext = createContext<NotificationRefreshValue | null>(null);

export function NotificationRefreshProvider({ children }: { children: React.ReactNode }) {
  const [refreshKey, setRefreshKey] = useState(0);
  const bumpRefresh = useCallback(() => setRefreshKey(k => k + 1), []);
  return (
    <NotificationRefreshContext.Provider value={{ refreshKey, bumpRefresh }}>
      {children}
    </NotificationRefreshContext.Provider>
  );
}

export function useNotificationRefresh() {
  const ctx = useContext(NotificationRefreshContext);
  if (!ctx) throw new Error('useNotificationRefresh must be used within NotificationRefreshProvider');
  return ctx;
}
