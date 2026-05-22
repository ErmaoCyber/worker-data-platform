import Sidebar from '@/component/sidebar/Sidebar';
import type { ReactNode } from 'react';

export default function WorkerLayout({
  children,
}: {
  children: ReactNode;
}) {
  return (
    <div className="flex h-screen overflow-hidden bg-gray-50">
      <Sidebar role="worker" />

      <main className="h-screen flex-1 overflow-y-auto p-6">
        {children}
      </main>
    </div>
  );
}
