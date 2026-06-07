'use client';

import { useEffect, useState } from 'react';
import {
  viewAccessItem,
  ViewAccessError,
  type AccessViewResult,
} from '@/lib/api/employerActiveAccessApi';

interface AccessViewModalProps {
  permissionId: string;
  itemLabel: string;
  itemType: 'text' | 'file';
  onClose: () => void;
}

// Supabase serves signed-URL responses with X-Frame-Options: SAMEORIGIN,
// which blocks cross-origin iframe embedding. Fetch the file via JS,
// then render through a same-origin blob: URL.
function FileFrame({ signedUrl, title }: { signedUrl: string; title: string }) {
  const [blobUrl, setBlobUrl] = useState<string | null>(null);
  const [err, setErr] = useState('');

  useEffect(() => {
    let revoked = false;
    let createdUrl: string | null = null;

    fetch(signedUrl)
      .then((r) => {
        if (!r.ok) throw new Error(`Storage returned ${r.status}`);
        return r.blob();
      })
      .then((blob) => {
        if (revoked) return;
        createdUrl = URL.createObjectURL(blob);
        setBlobUrl(createdUrl);
      })
      .catch((e) => setErr(e instanceof Error ? e.message : String(e)));

    return () => {
      revoked = true;
      if (createdUrl) URL.revokeObjectURL(createdUrl);
    };
  }, [signedUrl]);

  if (err) {
    return (
      <p className="rounded-lg bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-700">
        Failed to load file: {err}
      </p>
    );
  }

  if (!blobUrl) {
    return <p className="text-sm text-slate-500">Loading file...</p>;
  }

  return (
    <iframe
      src={`${blobUrl}#toolbar=0&navpanes=0`}
      className="w-full rounded-lg border border-slate-200"
      style={{ height: '70vh' }}
      title={title}
    />
  );
}

export default function AccessViewModal({
  permissionId,
  itemLabel,
  itemType,
  onClose,
}: AccessViewModalProps) {
  const [result, setResult] = useState<AccessViewResult | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState('');

  useEffect(() => {
    async function load() {
      const token = localStorage.getItem('accessToken');
      if (!token) {
        setErrorMsg('Not authenticated.');
        setIsLoading(false);
        return;
      }
      try {
        const data = await viewAccessItem(token, permissionId);
        setResult(data);
      } catch (err) {
        if (err instanceof ViewAccessError) {
          if (err.status === 503) {
            setErrorMsg('File storage is not configured. Please contact your administrator.');
          } else if (err.status === 404) {
            setErrorMsg('Permission not found.');
          } else if (err.status === 422) {
            setErrorMsg(err.detail || 'Cannot view this data right now.');
          } else if (err.status === 403) {
            setErrorMsg('You do not have access to this data.');
          } else {
            setErrorMsg('Failed to load data.');
          }
        } else {
          setErrorMsg('Failed to load data.');
        }
      } finally {
        setIsLoading(false);
      }
    }
    load();
  }, [permissionId]);

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
      onClick={onClose}
    >
      <div
        onClick={(e) => e.stopPropagation()}
        className="relative w-full max-w-4xl bg-white rounded-2xl shadow-lg p-6 max-h-[90vh] overflow-y-auto"
      >
        <div className="flex items-center justify-between mb-4">
          <div>
            <h2 className="text-xl font-semibold text-slate-900">{itemLabel}</h2>
            <p className="text-xs text-slate-500 mt-1">
              {itemType === 'file' ? 'File document' : 'Text value'}
            </p>
          </div>
          <button
            onClick={onClose}
            className="text-slate-400 hover:text-slate-600 text-2xl leading-none"
          >
            ✕
          </button>
        </div>

        {isLoading && <p className="text-sm text-slate-500">Loading data...</p>}

        {!isLoading && errorMsg && (
          <p className="rounded-lg bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-700">
            {errorMsg}
          </p>
        )}

        {!isLoading && !errorMsg && result && (
          <div>
            {result.type === 'text' && (
              <div className="rounded-lg bg-slate-50 border border-slate-200 px-4 py-3">
                <pre className="text-sm text-slate-800 whitespace-pre-wrap break-words font-sans">
                  {result.value ?? '(empty)'}
                </pre>
              </div>
            )}

            {result.type === 'file' && result.url && (
              <div>
                <FileFrame signedUrl={result.url} title={itemLabel} />
                {result.urlExpiresAt && (
                  <p className="mt-2 text-xs text-slate-400">
                    Link expires at {new Date(result.urlExpiresAt).toLocaleTimeString()}
                  </p>
                )}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
