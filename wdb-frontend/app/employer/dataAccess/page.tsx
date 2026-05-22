'use client';

import { useState } from 'react';
import MyRequestsTab from './MyRequestsTab';
import MyAccessTab from './MyAccessTab';

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
    <main className="p-8">
      <div>
        <h1 className="mb-6 text-2xl font-semibold text-gray-900">
          Data Access
        </h1>
      </div>

      <div className="flex border-b border-gray-200">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            type="button"
            onClick={() => setActiveTab(tab.id)}
            className={`
              -mb-px border-b-2 px-5 py-2.5 text-sm font-medium transition-colors cursor-pointer
              ${activeTab === tab.id
                ? 'border-gray-900 text-gray-900'
                : 'border-transparent text-gray-500 hover:text-gray-700'
              }
            `}
          >
            {tab.label}
          </button>
        ))}
      </div>

      <div className="mt-6">
        {activeTab === 'my-requests' && <MyRequestsTab />}
        {activeTab === 'my-access' && <MyAccessTab />}
      </div>
    </main>
  );
}
