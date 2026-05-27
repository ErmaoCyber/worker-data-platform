'use client';
import { FetchApi } from '@/lib/api';
import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/context/AuthContext';

interface RequestModalProps {
  onClose: () => void;
}

// Predefined categories with display info
const CATEGORIES = [
  {
    key: 'PersonaInformation',
    label: 'Basic personal details',
    desc: 'Name, date of birth, gender, contact details, address',
  },
  {
    key: 'MedicalInformation',
    label: 'Medical history',
    desc: 'Past illnesses, surgeries, hospital visits',
  },
  {
    key: 'CareerInformation',
    label: 'Career information',
    desc: 'Work experience, work role, achievements, location, duration',
  },
];

export default function RequestModal({ onClose }: RequestModalProps) {
  const router = useRouter();
  const { token, role, isAuthReady } = useAuth();

  type WorkerInfo = {
    id: string;
    desc: string;
    value: string;
    status: string;
    category: string;
  };

  type Worker = {
    id: string;
    name: string;
    email: string;
  };

  const [email, setEmail] = useState('');
  const [reason, setReason] = useState('');
  const [worker, setWorker] = useState<Worker | null>(null);
  const [workerInfos, setWorkerInfos] = useState<WorkerInfo[]>([]);
  const [workerInfosHasRequested, setWorkerInfosHasRequested] = useState<WorkerInfo[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [errorMsg, setErrorMsg] = useState('');
  const [sentMsg, setSentMsg] = useState('');
  const [findWorker, setFindWorker] = useState(false);
  const [isSelected, setSelected] = useState<Set<string>>(new Set());

  // Flexible request state
  const [newFlexRequestDesc, setNewFlexRequestDesc] = useState('');
  const [newDescCategory, setNewDescCategory] = useState('');
  const [showFlexRequest, setShowFlexRequest] = useState(false);

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

  async function handleSearch(email: string) {
    if (!email) {
      alert('Please enter an email');
      return;
    }
    if (!ensureEmployerAuth()) return;

    setIsLoading(true);
    setErrorMsg('');
    setSentMsg('');

    try {
      const worker = await FetchApi(
        `/api/Employer/GetWorkerByEmail?email=${email}`,
        { method: 'GET', headers: { Authorization: `Bearer ${token}` } }
      );
      if (!worker) {
        alert('There is no worker information for this email');
        return;
      }
      setWorker(worker);

      const workerInfos = await FetchApi(`/api/Employer?email=${email}`, {
        method: 'GET',
        headers: { Authorization: `Bearer ${token}` },
      });
      setWorkerInfos(workerInfos);

      const requestedWorkerInfos = await FetchApi(
        `/api/Employer/GetRequestedWorkerInfosByEmail?email=${email}`,
        { method: 'GET', headers: { Authorization: `Bearer ${token}` } }
      );
      setWorkerInfosHasRequested(requestedWorkerInfos);
      setFindWorker(true);
    } catch (error) {
      console.error('Failed to search worker:', error);
      setErrorMsg('Worker not found');
    } finally {
      setIsLoading(false);
    }
  }

  function toggleCategory(categoryKey: string) {
    setSelected((prev) => {
      const next = new Set(prev);
      next.has(categoryKey) ? next.delete(categoryKey) : next.add(categoryKey);
      return next;
    });
  }

  async function handleRequest() {
    if (!ensureEmployerAuth()) return;

    if (isSelected.size === 0 && !(newFlexRequestDesc && newDescCategory)) {
      alert('Please select at least one category or fill in a flexible request');
      return;
    }
    if (!reason) {
      alert('Please fill in the reason');
      return;
    }

    setSentMsg('Sending...');

    try {
      if (isSelected.size > 0) {
        // Get all workerInfo IDs that belong to selected categories
        const selectedIds = workerInfos
          .filter((w) => isSelected.has(w.category))
          .map((w) => w.id);

        await FetchApi('/api/Employer/AccessRequests', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${token}`,
          },
          body: JSON.stringify({
            Email: worker?.email,
            InfoDesc: selectedIds,
            Reason: reason,
          }),
        });
      }

      if (newFlexRequestDesc && newDescCategory) {
        await FetchApi('/api/Employer/AddFlexibleWorkerInfo', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${token}`,
          },
          body: JSON.stringify({
            WorkerEmail: worker?.email,
            Desc: newFlexRequestDesc,
            Category: newDescCategory,
            Reason: reason,
          }),
        });
      }

      setSentMsg('Request sent!');
    } catch (error) {
      console.error('Failed to submit request:', error);
      setSentMsg('Failed to send request');
    }
  }

  if (!isAuthReady) {
    return (
      <main className="p-8">
        <p className="text-gray-500">Loading request page...</p>
      </main>
    );
  }

  if (!token || role !== 'employer') {
    return null;
  }

  return (
    <div className="relative w-full max-w-2xl mx-auto p-8 bg-white rounded-2xl shadow-lg">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-2xl font-bold text-slate-900">Create new request</h2>
        <button onClick={onClose} className="text-slate-400 hover:text-slate-600 text-2xl">✕</button>
      </div>

      {/* Step 1: Search */}
      {!findWorker && (
        <div className="flex flex-col gap-4">
          <div className="relative border border-gray-300 rounded-xl px-4 pt-5 pb-2 w-full">
            <label className="absolute top-2 left-4 text-xs text-gray-400">Worker Email</label>
            <input
              type="text"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full outline-none text-gray-800"
              placeholder="worker@example.com"
            />
          </div>
          <button
            onClick={() => handleSearch(email)}
            disabled={isLoading}
            className="bg-slate-800 px-6 py-3 rounded-xl text-white w-full font-medium disabled:opacity-70"
          >
            {isLoading ? 'Searching...' : 'Search'}
          </button>
          {errorMsg && <p className="text-red-500 text-center">{errorMsg}</p>}
        </div>
      )}

      {/* Step 2: Select categories */}
      {findWorker && (
        <div className="flex flex-col gap-4">
          {/* Worker info */}
          <div className="bg-slate-50 rounded-xl p-4 text-sm text-slate-600">
            <p>Email: <span className="text-blue-600">{worker?.email}</span></p>
            <p>Name: {worker?.name}</p>
          </div>

          {/* Already requested */}
          <div>
            <p className="text-sm text-slate-600 mb-2">The info you have already requested:</p>
            {workerInfosHasRequested.length === 0 ? (
              <p className="text-sm text-slate-400">No previous requests for this worker.</p>
            ) : (
              <div className="flex flex-col gap-2">
                {workerInfosHasRequested.map((w) => (
                  <div key={w.id} className="border border-slate-200 rounded-lg px-4 py-2 text-sm text-slate-700">
                    {w.desc}: <span className="font-medium">{w.status}</span>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Category checkboxes */}
          <div>
            <p className="font-semibold text-slate-800 mb-3">Please choose the info you want to request:</p>
            <div className="flex flex-col gap-3">
              {CATEGORIES.map((cat) => (
                <label
                  key={cat.key}
                  className={`flex items-start gap-4 border rounded-xl px-4 py-4 cursor-pointer transition-colors ${isSelected.has(cat.key)
                    ? 'border-slate-800 bg-slate-50'
                    : 'border-slate-200 hover:border-slate-300'
                    }`}
                >
                  <input
                    type="checkbox"
                    className="mt-1 accent-slate-800"
                    checked={isSelected.has(cat.key)}
                    onChange={() => toggleCategory(cat.key)}
                  />
                  <div>
                    <p className="font-semibold text-slate-800">{cat.label}</p>
                    <p className="text-sm text-slate-400">{cat.desc}</p>
                  </div>
                </label>
              ))}
            </div>
          </div>

          {/* Flexible request */}
          <div>
            <button
              onClick={() => setShowFlexRequest(!showFlexRequest)}
              className="text-sm text-slate-500 underline"
            >
              {showFlexRequest ? 'Hide' : '+ Add flexible request (Optional)'}
            </button>

            {showFlexRequest && (
              <div className="flex gap-3 mt-3">
                <div className="relative border border-gray-300 rounded-xl px-4 pt-5 pb-2 flex-1">
                  <label className="absolute top-2 left-4 text-xs text-gray-400">Category</label>
                  <select
                    className="w-full outline-none text-gray-800"
                    value={newDescCategory}
                    onChange={(e) => setNewDescCategory(e.target.value)}
                  >
                    <option value="">Select category</option>
                    <option value="PersonaInformation">Personal Information</option>
                    <option value="MedicalInformation">Medical Information</option>
                    <option value="CareerInformation">Career Information</option>
                    <option value="OtherInformation">Other Information</option>
                  </select>
                </div>
                <div className="relative border border-gray-300 rounded-xl px-4 pt-5 pb-2 flex-1">
                  <label className="absolute top-2 left-4 text-xs text-gray-400">Description</label>
                  <input
                    type="text"
                    className="w-full outline-none text-gray-800"
                    value={newFlexRequestDesc}
                    onChange={(e) => setNewFlexRequestDesc(e.target.value)}
                  />
                </div>
              </div>
            )}
          </div>

          {/* Reason */}
          <div className="relative border border-gray-300 rounded-xl px-4 pt-5 pb-2 w-full">
            <label className="absolute top-2 left-4 text-xs text-gray-400">Reason</label>
            <input
              type="text"
              className="w-full outline-none text-gray-800"
              placeholder="Why do you need this information?"
              value={reason}
              onChange={(e) => setReason(e.target.value)}
            />
          </div>

          {/* Submit */}
          <button
            onClick={handleRequest}
            className="bg-slate-800 px-6 py-3 rounded-xl text-white w-full font-medium"
          >
            Submit
          </button>

          {sentMsg && <p className="text-center text-sm text-slate-600">{sentMsg}</p>}
        </div>
      )}
    </div>
  );
}