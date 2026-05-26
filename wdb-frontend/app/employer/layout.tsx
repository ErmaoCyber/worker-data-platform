import NotificationBell from "@/app/notification/NotificationBell"

// Employer layout: shared sidebar navigation for all employer pages
export default function EmployerLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen flex flex-col">
      <header className="flex justify-end items-center px-6 py-3 border-b border-gray-200 bg-white">
        <NotificationBell />
      </header>
      <div className="flex-1">
        {children}
      </div>
    </div>
  );
}