'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import type { LucideIcon } from 'lucide-react';

type SidebarItemProps = {
  label: string;
  icon: LucideIcon;
  href: string;
};

export default function SidebarItem({
  label,
  icon: Icon,
  href,
}: SidebarItemProps) {
  const pathname = usePathname();

  // Keep the nav item active when the user is on a child route.
  // Example: /employer/dataAccess/detail still highlights /employer/dataAccess.
  const isActive = pathname === href || pathname.startsWith(`${href}/`);

  return (
    <Link
      href={href}
      className={`
        flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium
        transition-colors duration-200
        ${isActive
          ? 'bg-blue-600 text-white'
          : 'text-gray-300 hover:bg-gray-700 hover:text-white'
        }
      `}
    >
      <Icon size={20} />
      <span>{label}</span>
    </Link>
  );
}
