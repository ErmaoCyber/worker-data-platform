import NotificationBell from "@/app/notification/NotificationBell";
import Sidebar from "@/component/sidebar/Sidebar";

export default function EmployerLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="flex h-screen overflow-hidden bg-gray-50">
      <Sidebar role="employer" />

      <div className="flex flex-1 flex-col">
        <header className="flex justify-end items-center px-6 py-3 border-b border-gray-200 bg-white">
          <NotificationBell />
        </header>

        <main className="flex-1 overflow-y-auto p-6">
          {children}
        </main>
      </div>
    </div>
  );
}
