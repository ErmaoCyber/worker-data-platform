'use client';

import { useState } from 'react';
import MyRequestsTab from './MyRequestsTab';

export default function EmployerDataAccessPage() {
  const [activeTab, setActiveTab] = useState('my-requests');

  const tabs = [
    {
      id: 'my-requests',
      label: 'My Requests',
    },
    {
      id: 'my-access',
      label: 'My Access',
    },
  ];

  return (
    <main className="min-h-screen bg-slate-50 px-8 py-8">
      <div>
        <h1 className="text-2xl font-semibold text-slate-900">
          Data Access
        </h1>
        <p className="mt-1 text-sm text-slate-500">
          View and manage your worker data requests and approved access.
        </p>
      </div>

      <div className="mt-6 flex border-b border-slate-200">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(tab.id)}
            className={`
              -mb-px border-b-2 px-5 py-2.5 text-sm font-medium transition-colors
              ${activeTab === tab.id
                ? 'border-slate-900 text-slate-900'
                : 'border-transparent text-slate-500 hover:text-slate-700'
              }
            `}
          >
            {tab.label}
          </button>
        ))}
      </div>

      <div className="mt-6">
        {activeTab === 'my-requests' && <MyRequestsTab />}

        {activeTab === 'my-access' && (
          <div className="rounded-2xl border border-slate-200 bg-white p-6 text-sm text-slate-500">
            My Access will be added in a later story.
          </div>
        )}
      </div>
    </main>
  );
}
