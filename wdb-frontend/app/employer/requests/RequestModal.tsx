'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/context/AuthContext';
import {
  createEmployerRequest,
  fetchRequestCatalog,
  type EmployerRequestCatalog,
} from '@/lib/api/employerRequestApi';

interface RequestModalProps {
  onClose: () => void;
}

export default function RequestModal({ onClose }: RequestModalProps) {
  const router = useRouter();
  const { token, role, isAuthReady } = useAuth();

  // Step 1 (search) state
  const [email, setEmail] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  // Step 2 (build request) state
  const [catalog, setCatalog] = useState<EmployerRequestCatalog | null>(null);
  const [reason, setReason] = useState('');
  const [expiryDate, setExpiryDate] = useState('');
  const [selectedFieldIds, setSelectedFieldIds] = useState<Set<string>>(new Set());
  const [selectedCustomItemIds, setSelectedCustomItemIds] = useState<Set<string>>(new Set());
  const [customRequest, setCustomRequest] = useState('');
  const [showCustomRequest, setShowCustomRequest] = useState(false);

  const [errorMsg, setErrorMsg] = useState('');
  const [sentMsg, setSentMsg] = useState('');

  function ensureEmployerAuth() {
    if (!isAuthReady) {
      setErrorMsg('Loading authentication. Please try again in a moment.');
      return false;
    }
    if (!token || role !== 'employer') {
      router.push('/login');
      return false;
    }
    return true;
  }

  async function handleSearch() {
    if (!email) {
      setErrorMsg('Please enter a worker email');
      return;
    }
    if (!ensureEmployerAuth()) return;

    setIsLoading(true);
    setErrorMsg('');
    setSentMsg('');

    try {
      const result = await fetchRequestCatalog(token!, email);
      setCatalog(result);
    } catch (err) {
      console.error('Failed to load catalog:', err);
      setErrorMsg('Worker not found');
    } finally {
      setIsLoading(false);
    }
  }

  function toggleFieldId(id: string) {
    setSelectedFieldIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  function toggleCustomItemId(id: string) {
    setSelectedCustomItemIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  async function handleSubmit() {
    if (!ensureEmployerAuth() || !catalog) return;

    if (!reason.trim()) {
      setErrorMsg('Please provide a reason');
      return;
    }
    if (!expiryDate) {
      setErrorMsg('Please choose an expiry date');
      return;
    }
    if (
      selectedFieldIds.size === 0 &&
      selectedCustomItemIds.size === 0 &&
      !customRequest.trim()
    ) {
      setErrorMsg('Please select at least one field, custom item, or write a custom request');
      return;
    }

    setSentMsg('Sending...');
    setErrorMsg('');

    try {
      await createEmployerRequest(token!, {
        workerEmail: catalog.worker.email,
        reason,
        expiryDate: new Date(expiryDate).toISOString(),
        presetFieldIds: Array.from(selectedFieldIds),
        customWorkerInfoIds: Array.from(selectedCustomItemIds),
        customRequest: customRequest.trim() ? customRequest.trim() : undefined,
      });
      setSentMsg('Request sent!');
    } catch (err) {
      console.error('Failed to submit request:', err);
      setSentMsg('');
      setErrorMsg('Failed to send request');
    }
  }

  if (!isAuthReady) {
    return (
      <div className="p-8">
        <p className="text-slate-500">Loading...</p>
      </div>
    );
  }

  if (!token || role !== 'employer') return null;

  return (
    <div className="relative w-full max-w-2xl mx-auto p-8 bg-white rounded-2xl shadow-lg max-h-[90vh] overflow-y-auto">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-2xl font-bold text-slate-900">Create new request</h2>
        <button onClick={onClose} className="text-slate-400 hover:text-slate-600 text-2xl">
          ✕
        </button>
      </div>

      {/* Step 1: Worker search */}
      {!catalog && (
        <div className="flex flex-col gap-4">
          <div className="relative border border-gray-300 rounded-xl px-4 pt-5 pb-2">
            <label className="absolute top-2 left-4 text-xs text-gray-400">Worker Email</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full outline-none text-gray-800"
              placeholder="worker@example.com"
            />
          </div>
          <button
            onClick={handleSearch}
            disabled={isLoading}
            className="bg-slate-800 px-6 py-3 rounded-xl text-white w-full font-medium disabled:opacity-70"
          >
            {isLoading ? 'Searching...' : 'Search'}
          </button>
          {errorMsg && <p className="text-red-500 text-center">{errorMsg}</p>}
        </div>
      )}

      {/* Step 2: Build request */}
      {catalog && (
        <div className="flex flex-col gap-5">
          <div className="bg-slate-50 rounded-xl p-4 text-sm text-slate-600">
            <p>
              Email: <span className="text-blue-600">{catalog.worker.email}</span>
            </p>
            <p>Name: {catalog.worker.name}</p>
          </div>

          {/* Categories with preset fields and (for Other) custom items */}
          {catalog.categories.map((category) => {
            const hasContent =
              category.presetFields.length > 0 || category.customItems.length > 0;
            if (!hasContent) return null;
            return (
              <div key={category.id}>
                <p className="font-semibold text-slate-800 mb-2">{category.name}</p>
                <div className="grid grid-cols-2 gap-2">
                  {category.presetFields.map((field) => (
                    <label
                      key={field.fieldId}
                      className="flex items-center gap-2 border rounded-lg px-3 py-2 cursor-pointer hover:border-slate-400"
                    >
                      <input
                        type="checkbox"
                        className="accent-slate-800"
                        checked={selectedFieldIds.has(field.fieldId)}
                        onChange={() => toggleFieldId(field.fieldId)}
                      />
                      <span className="text-sm text-slate-700">
                        {field.label}
                        {field.allowedType === 'file' && (
                          <span className="ml-1 text-xs text-slate-400">(file)</span>
                        )}
                      </span>
                    </label>
                  ))}
                  {category.customItems.map((item) => (
                    <label
                      key={item.workerInfoId}
                      className="flex items-center gap-2 border rounded-lg px-3 py-2 cursor-pointer hover:border-slate-400"
                    >
                      <input
                        type="checkbox"
                        className="accent-slate-800"
                        checked={selectedCustomItemIds.has(item.workerInfoId)}
                        onChange={() => toggleCustomItemId(item.workerInfoId)}
                      />
                      <span className="text-sm text-slate-700">
                        {item.label}
                        {item.type === 'file' && (
                          <span className="ml-1 text-xs text-slate-400">(file)</span>
                        )}
                      </span>
                    </label>
                  ))}
                </div>
              </div>
            );
          })}

          {/* Optional free-text custom request */}
          <div>
            <button
              onClick={() => setShowCustomRequest((v) => !v)}
              className="text-sm text-slate-500 underline"
            >
              {showCustomRequest ? 'Hide custom request' : '+ Add custom request (Optional)'}
            </button>
            {showCustomRequest && (
              <textarea
                className="mt-3 w-full border border-gray-300 rounded-xl px-4 py-3 outline-none text-gray-800"
                rows={3}
                placeholder="Describe any extra information you need that is not in the list above"
                value={customRequest}
                onChange={(e) => setCustomRequest(e.target.value)}
              />
            )}
          </div>

          {/* Reason */}
          <div className="relative border border-gray-300 rounded-xl px-4 pt-5 pb-2">
            <label className="absolute top-2 left-4 text-xs text-gray-400">Reason</label>
            <input
              type="text"
              className="w-full outline-none text-gray-800"
              placeholder="Why do you need this information?"
              value={reason}
              onChange={(e) => setReason(e.target.value)}
            />
          </div>

          {/* Expiry date */}
          <div className="relative border border-gray-300 rounded-xl px-4 pt-5 pb-2">
            <label className="absolute top-2 left-4 text-xs text-gray-400">Expiry Date</label>
            <input
              type="datetime-local"
              className="w-full outline-none text-gray-800"
              value={expiryDate}
              onChange={(e) => setExpiryDate(e.target.value)}
            />
          </div>

          <button
            onClick={handleSubmit}
            className="bg-slate-800 px-6 py-3 rounded-xl text-white w-full font-medium"
          >
            Submit
          </button>

          {errorMsg && <p className="text-red-500 text-center">{errorMsg}</p>}
          {sentMsg && <p className="text-center text-sm text-slate-600">{sentMsg}</p>}
        </div>
      )}
    </div>
  );
}
