import Sidebar from '@/component/sidebar/Sidebar';

export default function EmployerLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="flex h-screen overflow-hidden bg-gray-50">
      <Sidebar role="employer" />

      <main className="h-screen flex-1 overflow-y-auto p-6">
        {children}
      </main>
    </div>
  );
}
