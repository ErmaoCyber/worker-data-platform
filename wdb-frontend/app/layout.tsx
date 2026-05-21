import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import SignalRListener from "./notification/SignalRListener";
import 'react-toastify/dist/ReactToastify.css';
import { UserProvider } from "@/lib/api/userContext";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Worker Data Blockchain",
  description: "Blockchain-based worker data authorization platform",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className={`${geistSans.variable} ${geistMono.variable}`}>
      <body className="m-0 min-h-screen flex flex-col">
        {/* Previously rendered {children} and <SignalRListener /> directly at body root.
            Wrapped in <UserProvider> so notification components and the SignalR listener
            can consume the centralized user state via useUser(). */}
        <UserProvider>
          {children}
          <SignalRListener />
        </UserProvider>
      </body>
    </html>
  );
}



