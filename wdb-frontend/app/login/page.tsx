'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { login, UserRole } from '@/lib/api';
import { useAuth } from '@/context/AuthContext';
import heroBackground from './../assets/heroBackground.png';

export default function LoginPage() {
  const router = useRouter();

  const [role, setRole] = useState<UserRole>('worker');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  // Rename context login to avoid conflict with the API login function
  const { login: authLogin } = useAuth();

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    setError('');
    setLoading(true);

    try {
      const res = await login(role, email, password);

      if (!res.success || !res.data) {
        setError(res.message);
        return;
      }

      // Save login data into AuthContext and localStorage
      authLogin(
        res.data.accessToken,
        res.data.userName,
        res.data.email,
        res.data.userId,
        role
      );

      router.push(`/${role}/dashboard`);
    } catch {
      setError('Network error. Please try again.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="relative min-h-screen flex items-center justify-center p-4 bg-gray-700">
      <div
        className="absolute inset-0 bg-cover bg-center bg-no-repeat opacity-30"
        style={{ backgroundImage: `url(${heroBackground.src})` }}
      />

      <div className="z-10 bg-white border border-[#D9D9D9] rounded-xl p-10 w-full max-w-[400px]">
        <h1 className="text-2xl font-bold text-black mt-0 mb-8">Sign In</h1>

        <form onSubmit={handleSubmit} className="flex flex-col gap-5">
          <div>
            <label className="block text-sm font-semibold text-black mb-1.5">
              Email
            </label>

            <input
              type="email"
              placeholder="you@example.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="w-full py-[11px] px-[14px] border border-[#D9D9D9] rounded-lg text-[0.9rem] text-black bg-white outline-none"
            />
          </div>

          <div>
            <label className="block text-sm font-semibold text-black mb-1.5">
              Password
            </label>

            <input
              type="password"
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className="w-full py-[11px] px-[14px] border border-[#D9D9D9] rounded-lg text-[0.9rem] text-black bg-white outline-none"
            />
          </div>

          <div>
            <label className="block text-sm font-semibold text-black mb-1.5">
              Role
            </label>

            <div className="relative">
              <select
                value={role}
                onChange={(e) => setRole(e.target.value as UserRole)}
                className="w-full py-[11px] px-[14px] border border-[#D9D9D9] rounded-lg text-[0.9rem] text-black bg-white outline-none cursor-pointer appearance-none"
              >
                <option value="worker">Worker</option>
                <option value="employer">Employer</option>
              </select>

              <span className="absolute right-[14px] top-1/2 -translate-y-1/2 text-[#D9D9D9] pointer-events-none text-[0.7rem]">
                ▼
              </span>
            </div>
          </div>

          {error && (
            <p
              className="text-black text-[0.85rem] m-0 pl-[10px]"
              style={{ borderLeft: '3px solid #D9D9D9' }}
            >
              {error}
            </p>
          )}

          <button
            type="submit"
            disabled={loading}
            className="bg-[#49454F] text-white p-3 rounded-lg text-[0.9rem] font-semibold mt-1 cursor-pointer disabled:cursor-not-allowed disabled:opacity-70"
          >
            {loading ? 'Signing in...' : 'Sign In'}
          </button>
        </form>

        <p className="text-sm text-[#49454F] mt-6 mb-0">
          Don&apos;t have an account?{' '}
          <Link href="/register" className="text-black font-semibold underline">
            Register
          </Link>
        </p>
      </div>
    </div>
  );
}
