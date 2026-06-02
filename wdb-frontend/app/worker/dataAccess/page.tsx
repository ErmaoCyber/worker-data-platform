'use client';

import { ReactNode, useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/context/AuthContext';
import ActiveRequestTab from './ActiveRequestTab';
import ActiveAccessTab from './ActiveAccessTab';

interface TabProps {
  id: string;
  label: string;
  children: ReactNode;
}

export default function Page() {
  const router = useRouter();
  const { token, role, isAuthReady } = useAuth();

  const [activeTab, setActiveTab] = useState<string>('active-request');
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    if (!isAuthReady) return;

    if (!token || role !== 'worker') {
      router.push('/login');
    }
  }, [isAuthReady, token, role, router]);

  if (!isAuthReady) {
    return (
      <main className="min-h-screen bg-slate-50 px-8 py-8">
        <p className="text-sm text-slate-500">Loading data access...</p>
      </main>
    );
  }

  if (!token || role !== 'worker') {
    return null;
  }

  const handleChanged = () => {
    setRefreshKey((current) => current + 1);
  };

  const tabs: TabProps[] = [
    {
      id: 'active-request',
      label: 'Active Requests',
      children: (
        <ActiveRequestTab
          token={token}
          refreshKey={refreshKey}
          onChanged={handleChanged}
        />
      ),
    },
    {
      id: 'active-access',
      label: 'Active Access',
      children: (
        <ActiveAccessTab
          token={token}
          refreshKey={refreshKey}
          onChanged={handleChanged}
        />
      ),
    },
  ];

  const activeContent = tabs.find((tab) => tab.id === activeTab)?.children;

  return (
    <main className="min-h-screen bg-slate-50 px-8 py-8">
      <div className="mx-auto max-w-7xl space-y-6">
        <header>
          <p className="text-sm font-medium text-slate-500">Worker portal</p>
          <h1 className="mt-1 text-2xl font-semibold text-slate-900">
            Data Access
          </h1>
          <p className="mt-1 text-sm text-slate-500">
            Review company requests, add requested information when needed, and
            revoke access you no longer want to share.
          </p>
        </header>

        <div className="border-b border-slate-200">
          <div className="flex gap-6">
            {tabs.map(({ id, label }) => (
              <button
                key={id}
                onClick={() => setActiveTab(id)}
                className={`border-b-2 px-1 pb-3 text-sm font-semibold transition-colors ${activeTab === id
                    ? 'border-blue-600 text-blue-700'
                    : 'border-transparent text-slate-500 hover:text-slate-800'
                  }`}
              >
                {label}
              </button>
            ))}
          </div>
        </div>

        <section>{activeContent}</section>
      </div>
    </main>
  );
}
