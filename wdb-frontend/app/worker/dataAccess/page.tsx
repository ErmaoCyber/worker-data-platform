'use client';

// Worker requests: view, approve, or reject employer data access requests
import { ReactNode, useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/context/AuthContext';
import ActiveRequestTab from './ActiveRequestTab';
import ActiveAccessTab from './ActiveAccessTab';

interface TabProps {
  id: string;
  label: string;
  children?: ReactNode;
}

export default function Page() {
  const router = useRouter();

  // Read auth state from AuthContext
  const { token, userId, role, isAuthReady } = useAuth();

  const [activeTab, setActiveTab] = useState<string>('active-request');

  useEffect(() => {
    // Wait until AuthContext finishes restoring auth data from localStorage
    if (!isAuthReady) {
      return;
    }

    // Only logged-in workers can access this page
    if (!token || !userId || role !== 'worker') {
      router.push('/login');
      return;
    }
  }, [isAuthReady, token, userId, role, router]);

  if (!isAuthReady) {
    return (
      <main className="p-8">
        <p className="text-gray-500">Loading data access...</p>
      </main>
    );
  }

  if (!token || !userId || role !== 'worker') {
    return null;
  }

  const tabs: TabProps[] = [
    {
      id: 'active-request',
      label: 'Active Request',
      children: (
        <div>
          <ActiveRequestTab />
        </div>
      ),
    },
    {
      id: 'active-access',
      label: 'Active Access',
      children: (
        <div>
          <ActiveAccessTab />
        </div>
      ),
    },
  ];

  const activeContent = tabs.find((tab) => tab.id === activeTab)?.children;

  return (
    <main className="p-8">
      <div>
        <h1 className="text-2xl font-semibold mb-6 text-gray-900">
          Data Access
        </h1>
      </div>

      <div className="flex border-b border-gray-200">
        {tabs.map(({ id, label }) => (
          <button
            key={id}
            onClick={() => setActiveTab(id)}
            className={`
                            px-5 py-2.5 text-sm font-medium border-b-2 -mb-px transition-colors cursor-pointer
                            ${activeTab === id
                ? 'border-gray-900 text-gray-900'
                : 'border-transparent text-gray-500 hover:text-gray-700'
              }
                        `}
          >
            {label}
          </button>
        ))}
      </div>

      <div className="mt-6">
        {activeContent}
      </div>
    </main>
  );
}
