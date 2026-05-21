'use client';

import SidebarItem from './SidebarItem';

import {
  LayoutDashboard,
  KeyRound,
  ArrowLeftRight,
  User,
} from 'lucide-react';

type SidebarRole = 'worker' | 'employer';

type SidebarProps = {
  role: SidebarRole;
};

const workerNavItems = [
  {
    label: 'Dashboard',
    icon: LayoutDashboard,
    href: '/worker/dashboard',
  },
  {
    label: 'Data Access',
    icon: KeyRound,
    href: '/worker/dataAccess',
  },
  {
    label: 'Audit Log',
    icon: ArrowLeftRight,
    href: '/worker/audit-log',
  },
  {
    label: 'Personal Data',
    icon: User,
    href: '/worker/profile',
  },
];

const employerNavItems = [
  {
    label: 'Dashboard',
    icon: LayoutDashboard,
    href: '/employer/dashboard',
  },
  {
    label: 'Data Access',
    icon: KeyRound,
    href: '/employer/dataAccess',
  },
];

export default function Sidebar({ role }: SidebarProps) {
  const navItems = role === 'worker' ? workerNavItems : employerNavItems;

  return (
    <aside className="flex h-screen w-56 flex-col bg-gray-900 text-white">
      <div className="flex flex-col items-center border-b border-gray-700 px-4 py-8">
        <div className="mb-3 flex h-10 w-10 items-center justify-center rounded-full bg-blue-500">
          <span className="text-lg font-bold text-white">F</span>
        </div>

        <span className="text-center text-sm font-semibold text-white">
          First Step Solution
        </span>

        <span className="mt-1 text-xs text-gray-400">
          {role === 'worker' ? 'Worker Portal' : 'Employer Portal'}
        </span>
      </div>

      <nav className="flex flex-col gap-1 px-2 py-4">
        {navItems.map((item) => (
          <SidebarItem
            key={item.href}
            label={item.label}
            icon={item.icon}
            href={item.href}
          />
        ))}
      </nav>
    </aside>
  );
}
