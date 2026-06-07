'use client';

import { useEffect, useMemo, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/context/AuthContext';
import {
  createCustomField,
  deleteCustomField,
  getWorkerProfile,
  updateCustomField,
  updatePresetField,
  uploadProfileFile,
} from '@/lib/api/workerApi';
import type {
  WorkerProfileCategory,
  WorkerProfileField,
} from './type';

type DraftState = Record<
  string,
  {
    label: string;
    value: string;
  }
>;

type NewCustomState = {
  label: string;
  type: 'text' | 'file';
  value: string;
};

const PAGE_SIZE = 5;

function categoryTitle(category: string) {
  switch (category) {
    case 'PersonalInformation':
      return 'Personal Information';
    case 'MedicalInformation':
      return 'Medical Information';
    case 'CareerInformation':
      return 'Career Information';
    case 'FinancialInformation':
      return 'Financial Information';
    case 'WorkplaceInformation':
      return 'Workplace Information';
    case 'OtherInformation':
      return 'Other Information';
    default:
      return category || 'Other Information';
  }
}

function categoryDescription(category: string) {
  switch (category) {
    case 'PersonalInformation':
      return 'Basic identity and contact details used for onboarding.';
    case 'MedicalInformation':
      return 'Health and safety information that may be needed before site work.';
    case 'CareerInformation':
      return 'Work experience, skills, certifications, and employment-related details.';
    case 'FinancialInformation':
      return 'Payment, tax, and payroll-related information.';
    case 'WorkplaceInformation':
      return 'Site access, work preferences, and workplace-specific details.';
    case 'OtherInformation':
      return 'Extra information you choose to add yourself.';
    default:
      return 'Worker profile information.';
  }
}

function getFieldKey(field: WorkerProfileField) {
  return field.infoId ?? field.fieldId ?? field.label;
}

// Path stored in DB is "worker/{workerId}/{guid}-{originalFilename}".
// Strip the prefix and 36-char UUID to show only the human-readable name.
function fileNameFromPath(path: string) {
  const last = path.substring(path.lastIndexOf('/') + 1);
  return last.length > 37 ? last.substring(37) : last;
}

function sortCategories(categories: WorkerProfileCategory[]) {
  const order = [
    'PersonalInformation',
    'MedicalInformation',
    'CareerInformation',
    'FinancialInformation',
    'WorkplaceInformation',
    'OtherInformation',
  ];

  return [...categories].sort((a, b) => {
    const aIndex = order.indexOf(a.category);
    const bIndex = order.indexOf(b.category);

    const safeAIndex = aIndex === -1 ? 98 : aIndex;
    const safeBIndex = bIndex === -1 ? 98 : bIndex;

    return safeAIndex - safeBIndex;
  });
}

function getFilledCount(category: WorkerProfileCategory) {
  return category.fields.filter((field) => field.hasValue).length;
}

function getTotalCount(categories: WorkerProfileCategory[]) {
  return categories.reduce(
    (total, category) => total + category.fields.length,
    0,
  );
}

function getCompletedCount(categories: WorkerProfileCategory[]) {
  return categories.reduce(
    (total, category) =>
      total + category.fields.filter((field) => field.hasValue).length,
    0,
  );
}

export default function ProfilePage() {
  const router = useRouter();
  const { token, role, isAuthReady } = useAuth();

  const [categories, setCategories] = useState<WorkerProfileCategory[]>([]);
  const [selectedCategory, setSelectedCategory] =
    useState<string>('PersonalInformation');

  const [page, setPage] = useState(1);

  const [drafts, setDrafts] = useState<DraftState>({});
  const [editingKey, setEditingKey] = useState<string | null>(null);
  const [savingKey, setSavingKey] = useState<string | null>(null);

  const [showCustomForm, setShowCustomForm] = useState(false);
  const [newCustom, setNewCustom] = useState<NewCustomState>({
    label: '',
    type: 'text',
    value: '',
  });

  const [isLoading, setIsLoading] = useState(false);
  const [errorMsg, setErrorMsg] = useState('');
  const [successMsg, setSuccessMsg] = useState('');

  async function loadProfile(currentToken: string) {
    setIsLoading(true);
    setErrorMsg('');

    try {
      const data = await getWorkerProfile(currentToken);
      const sorted = sortCategories(data);

      setCategories(sorted);

      if (
        sorted.length > 0 &&
        !sorted.some((item) => item.category === selectedCategory)
      ) {
        setSelectedCategory(sorted[0].category);
      }

      const nextDrafts: DraftState = {};

      sorted.forEach((category) => {
        category.fields.forEach((field) => {
          const key = getFieldKey(field);

          nextDrafts[key] = {
            label: field.label ?? '',
            value: field.value ?? '',
          };
        });
      });

      setDrafts(nextDrafts);
    } catch (error) {
      setErrorMsg(error instanceof Error ? error.message : String(error));
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    if (!isAuthReady) {
      return;
    }

    if (!token || role !== 'worker') {
      router.push('/login');
      return;
    }

    loadProfile(token);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isAuthReady, token, role, router]);

  const sortedCategories = useMemo(
    () => sortCategories(categories),
    [categories],
  );

  const currentCategory = sortedCategories.find(
    (category) => category.category === selectedCategory,
  );

  const currentFields = currentCategory?.fields ?? [];
  const totalPages = Math.max(1, Math.ceil(currentFields.length / PAGE_SIZE));

  const pagedFields = currentFields.slice(
    (page - 1) * PAGE_SIZE,
    page * PAGE_SIZE,
  );

  const totalCount = getTotalCount(categories);
  const completedCount = getCompletedCount(categories);
  const completionPercent =
    totalCount === 0 ? 0 : Math.round((completedCount / totalCount) * 100);

  function handleSelectCategory(category: string) {
    setSelectedCategory(category);
    setPage(1);
    setEditingKey(null);
    setErrorMsg('');
    setSuccessMsg('');
    setShowCustomForm(false);
  }

  function updateDraft(
    field: WorkerProfileField,
    updates: Partial<{ label: string; value: string }>,
  ) {
    const key = getFieldKey(field);

    setDrafts((current) => ({
      ...current,
      [key]: {
        label: current[key]?.label ?? field.label ?? '',
        value: current[key]?.value ?? field.value ?? '',
        ...updates,
      },
    }));
  }

  function startEditing(field: WorkerProfileField) {
    const key = getFieldKey(field);

    setDrafts((current) => ({
      ...current,
      [key]: {
        label: field.label ?? '',
        value: field.value ?? '',
      },
    }));

    setEditingKey(key);
    setErrorMsg('');
    setSuccessMsg('');
  }

  function cancelEditing() {
    setEditingKey(null);
    setErrorMsg('');
  }

  async function handleSavePreset(field: WorkerProfileField) {
    if (!token || !field.fieldId) {
      return;
    }

    const key = getFieldKey(field);
    const value = drafts[key]?.value ?? '';

    setSavingKey(key);
    setErrorMsg('');
    setSuccessMsg('');

    try {
      await updatePresetField(token, {
        fieldId: field.fieldId,
        value: value.trim() === '' ? null : value.trim(),
      });

      await loadProfile(token);
      setEditingKey(null);
      setSuccessMsg('Information saved.');
    } catch (error) {
      setErrorMsg(error instanceof Error ? error.message : String(error));
    } finally {
      setSavingKey(null);
    }
  }

  async function handleSaveCustom(field: WorkerProfileField) {
    if (!token || !field.infoId) {
      return;
    }

    const key = getFieldKey(field);
    const label = drafts[key]?.label ?? field.label;
    const value = drafts[key]?.value ?? '';

    if (!label.trim()) {
      setErrorMsg('Custom field label is required.');
      return;
    }

    setSavingKey(key);
    setErrorMsg('');
    setSuccessMsg('');

    try {
      await updateCustomField(token, field.infoId, {
        label: label.trim(),
        value: value.trim() === '' ? null : value.trim(),
      });

      await loadProfile(token);
      setEditingKey(null);
      setSuccessMsg('Custom information saved.');
    } catch (error) {
      setErrorMsg(error instanceof Error ? error.message : String(error));
    } finally {
      setSavingKey(null);
    }
  }

  async function handleDeleteCustom(field: WorkerProfileField) {
    if (!token || !field.infoId) {
      return;
    }

    const confirmed = window.confirm(
      `Delete "${field.label}"? If this field has access history, the system may keep it for audit reasons.`,
    );

    if (!confirmed) {
      return;
    }

    const key = getFieldKey(field);

    setSavingKey(key);
    setErrorMsg('');
    setSuccessMsg('');

    try {
      await deleteCustomField(token, field.infoId);
      await loadProfile(token);
      setSuccessMsg('Custom information deleted.');
      setPage(1);
    } catch (error) {
      setErrorMsg(error instanceof Error ? error.message : String(error));
    } finally {
      setSavingKey(null);
    }
  }

  async function handleCreateCustom() {
    if (!token) {
      return;
    }

    if (!newCustom.label.trim()) {
      setErrorMsg('Please enter a label for the custom field.');
      return;
    }

    setSavingKey('new-custom');
    setErrorMsg('');
    setSuccessMsg('');

    try {
      await createCustomField(token, {
        label: newCustom.label.trim(),
        type: newCustom.type,
        value: newCustom.value.trim() === '' ? null : newCustom.value.trim(),
      });

      setNewCustom({
        label: '',
        type: 'text',
        value: '',
      });

      setShowCustomForm(false);
      setSelectedCategory('OtherInformation');
      setPage(1);

      await loadProfile(token);
      setSuccessMsg('Custom information added.');
    } catch (error) {
      setErrorMsg(error instanceof Error ? error.message : String(error));
    } finally {
      setSavingKey(null);
    }
  }

  if (!isAuthReady || isLoading) {
    return (
      <main className="min-h-screen bg-slate-50 px-8 py-8">
        <p className="text-sm text-slate-500">Loading profile...</p>
      </main>
    );
  }

  if (!token || role !== 'worker') {
    return null;
  }

  return (
    <main className="min-h-screen bg-slate-50 px-8 py-8">
      <div className="mx-auto max-w-7xl space-y-6">
        <header className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
          <div className="flex flex-col gap-5 lg:flex-row lg:items-start lg:justify-between">
            <div>
              <p className="text-sm font-medium text-slate-500">
                Worker portal
              </p>

              <h1 className="mt-1 text-2xl font-semibold text-slate-900">
                Personal Data
              </h1>

              <p className="mt-2 max-w-3xl text-sm text-slate-500">
                Manage the information companies may request from you. Missing
                values can stop you from approving a request later, but you stay
                in control of what is shared.
              </p>
            </div>

            <div className="w-full max-w-xs rounded-xl border border-slate-200 bg-slate-50 p-4">
              <div className="flex items-center justify-between text-sm">
                <span className="font-medium text-slate-700">
                  Profile completion
                </span>
                <span className="font-semibold text-slate-900">
                  {completionPercent}%
                </span>
              </div>

              <div className="mt-3 h-2 rounded-full bg-slate-200">
                <div
                  className="h-2 rounded-full bg-blue-600 transition-all"
                  style={{ width: `${completionPercent}%` }}
                />
              </div>

              <p className="mt-2 text-xs text-slate-500">
                {completedCount} of {totalCount} fields have saved values.
              </p>
            </div>
          </div>
        </header>

        {(errorMsg || successMsg) && (
          <section
            className={`rounded-xl border px-4 py-3 text-sm ${errorMsg
                ? 'border-red-200 bg-red-50 text-red-700'
                : 'border-emerald-200 bg-emerald-50 text-emerald-700'
              }`}
          >
            {errorMsg || successMsg}
          </section>
        )}

        <div className="grid items-start gap-6 lg:grid-cols-[280px_minmax(0,1fr)]">
          <aside className="h-[640px] overflow-y-auto rounded-2xl border border-slate-200 bg-white p-3 shadow-sm lg:sticky lg:top-6">
            <p className="px-3 pb-2 pt-1 text-xs font-semibold uppercase tracking-wide text-slate-400">
              Categories
            </p>

            <div className="space-y-1">
              {sortedCategories.map((category) => {
                const filled = getFilledCount(category);
                const total = category.fields.length;
                const isActive = selectedCategory === category.category;

                return (
                  <button
                    key={category.category}
                    onClick={() => handleSelectCategory(category.category)}
                    className={`w-full rounded-xl px-3 py-3 text-left transition-colors ${isActive
                        ? 'bg-blue-50 text-blue-700 ring-1 ring-blue-100'
                        : 'text-slate-600 hover:bg-slate-50 hover:text-slate-900'
                      }`}
                  >
                    <div className="flex items-center justify-between gap-3">
                      <span className="text-sm font-semibold">
                        {categoryTitle(category.category)}
                      </span>

                      <span
                        className={`rounded-full px-2 py-0.5 text-xs font-medium ${isActive
                            ? 'bg-blue-100 text-blue-700'
                            : 'bg-slate-100 text-slate-500'
                          }`}
                      >
                        {filled}/{total}
                      </span>
                    </div>

                    <p className="mt-1 line-clamp-2 text-xs text-slate-500">
                      {categoryDescription(category.category)}
                    </p>
                  </button>
                );
              })}
            </div>
          </aside>

          <section className="flex h-[640px] flex-col overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
            {currentCategory ? (
              <>
                <div className="shrink-0 flex flex-col gap-3 border-b border-slate-100 p-6 sm:flex-row sm:items-start sm:justify-between">
                  <div>
                    <h2 className="text-lg font-semibold text-slate-900">
                      {categoryTitle(currentCategory.category)}
                    </h2>

                    <p className="mt-1 text-sm text-slate-500">
                      {categoryDescription(currentCategory.category)}
                    </p>
                  </div>

                  {currentCategory.category === 'OtherInformation' && (
                    <button
                      onClick={() => setShowCustomForm((current) => !current)}
                      className="w-fit rounded-lg bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800"
                    >
                      {showCustomForm
                        ? 'Cancel custom field'
                        : 'Add custom field'}
                    </button>
                  )}
                </div>

                {currentCategory.category === 'OtherInformation' &&
                  showCustomForm && (
                    <div className="shrink-0 border-b border-slate-100 bg-slate-50 p-6">
                      <div className="grid gap-3 md:grid-cols-[1fr_150px_1fr_auto] md:items-end">
                        <div>
                          <label className="mb-1 block text-sm font-medium text-slate-700">
                            Label
                          </label>

                          <input
                            value={newCustom.label}
                            onChange={(event) =>
                              setNewCustom((current) => ({
                                ...current,
                                label: event.target.value,
                              }))
                            }
                            placeholder="e.g. Emergency site contact note"
                            className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-100"
                          />
                        </div>

                        <div>
                          <label className="mb-1 block text-sm font-medium text-slate-700">
                            Type
                          </label>

                          <select
                            value={newCustom.type}
                            onChange={(event) =>
                              setNewCustom((current) => ({
                                ...current,
                                type: event.target.value as 'text' | 'file',
                              }))
                            }
                            className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 focus:border-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-100"
                          >
                            <option value="text">Text</option>
                            <option value="file">File</option>
                          </select>
                        </div>

                        <div>
                          <label className="mb-1 block text-sm font-medium text-slate-700">
                            Value
                          </label>

                          {newCustom.type === 'file' ? (
                            <>
                              <input
                                type="file"
                                disabled={savingKey === 'new-custom'}
                                onChange={async (event) => {
                                  const file = event.target.files?.[0];
                                  if (!file || !token) return;
                                  setSavingKey('new-custom');
                                  setErrorMsg('');
                                  try {
                                    const path = await uploadProfileFile(token, file);
                                    setNewCustom((current) => ({
                                      ...current,
                                      value: path,
                                    }));
                                  } catch (error) {
                                    setErrorMsg(
                                      error instanceof Error
                                        ? error.message
                                        : String(error),
                                    );
                                  } finally {
                                    setSavingKey(null);
                                  }
                                }}
                                className="block w-full text-sm text-slate-700 file:mr-3 file:rounded-md file:border-0 file:bg-slate-100 file:px-3 file:py-1.5 file:text-sm file:font-medium file:text-slate-700 hover:file:bg-slate-200"
                              />
                              {newCustom.value && (
                                <p className="mt-1 text-xs text-slate-500">
                                  Uploaded: {fileNameFromPath(newCustom.value)}
                                </p>
                              )}
                            </>
                          ) : (
                            <input
                              value={newCustom.value}
                              onChange={(event) =>
                                setNewCustom((current) => ({
                                  ...current,
                                  value: event.target.value,
                                }))
                              }
                              placeholder="Enter the information to save"
                              className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-100"
                            />
                          )}
                        </div>

                        <button
                          disabled={savingKey === 'new-custom'}
                          onClick={handleCreateCustom}
                          className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-40"
                        >
                          Add
                        </button>
                      </div>
                    </div>
                  )}

                <div className="shrink-0 grid grid-cols-[260px_minmax(360px,1fr)_120px] gap-4 border-b border-slate-100 bg-slate-50 px-6 py-3 text-xs font-semibold uppercase tracking-wide text-slate-400">
                  <div>Field</div>
                  <div>Information</div>
                  <div className="text-right">Action</div>
                </div>

                <div className="flex-1 overflow-hidden">
                  {currentFields.length === 0 ? (
                    <div className="p-8 text-sm text-slate-500">
                      No fields in this category.
                    </div>
                  ) : (
                    <div className="divide-y divide-slate-100">
                      {pagedFields.map((field) => {
                        const key = getFieldKey(field);
                        const isEditing = editingKey === key;
                        const draft = drafts[key] ?? {
                          label: field.label,
                          value: field.value ?? '',
                        };

                        return (
                          <div
                            key={key}
                            className="grid h-[74px] grid-cols-[260px_minmax(360px,1fr)_120px] items-center gap-4 px-6"
                          >
                            <div className="min-w-0">
                              {isEditing && !field.isPreset ? (
                                <input
                                  value={draft.label}
                                  onChange={(event) =>
                                    updateDraft(field, {
                                      label: event.target.value,
                                    })
                                  }
                                  className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-900 focus:border-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-100"
                                />
                              ) : (
                                <p className="truncate text-sm font-semibold text-slate-900">
                                  {field.label}
                                </p>
                              )}

                              <div className="mt-1">
                                <span
                                  className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${field.hasValue
                                      ? 'bg-emerald-50 text-emerald-700'
                                      : 'bg-orange-50 text-orange-700'
                                    }`}
                                >
                                  {field.hasValue ? 'Saved' : 'Missing'}
                                </span>
                              </div>
                            </div>

                            <div className="min-w-0">
                              {isEditing ? (
                                field.type === 'file' ? (
                                  <div>
                                    <input
                                      type="file"
                                      disabled={savingKey === key}
                                      onChange={async (event) => {
                                        const file = event.target.files?.[0];
                                        if (!file || !token) return;
                                        setSavingKey(key);
                                        setErrorMsg('');
                                        try {
                                          const path = await uploadProfileFile(
                                            token,
                                            file,
                                          );
                                          updateDraft(field, { value: path });
                                        } catch (error) {
                                          setErrorMsg(
                                            error instanceof Error
                                              ? error.message
                                              : String(error),
                                          );
                                        } finally {
                                          setSavingKey(null);
                                        }
                                      }}
                                      className="block w-full text-sm text-slate-700 file:mr-3 file:rounded-md file:border-0 file:bg-slate-100 file:px-3 file:py-1.5 file:text-sm file:font-medium file:text-slate-700 hover:file:bg-slate-200"
                                    />
                                    {draft.value && (
                                      <p className="mt-1 text-xs text-slate-500">
                                        Current: {fileNameFromPath(draft.value)}
                                      </p>
                                    )}
                                  </div>
                                ) : (
                                  <input
                                    value={draft.value}
                                    onChange={(event) =>
                                      updateDraft(field, {
                                        value: event.target.value,
                                      })
                                    }
                                    placeholder="Leave blank to clear this value"
                                    className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-100"
                                  />
                                )
                              ) : field.hasValue ? (
                                field.type === 'file' ? (
                                  <p className="break-words text-sm text-slate-900">
                                    {fileNameFromPath(field.value ?? '')}
                                  </p>
                                ) : (
                                  <p className="break-words text-sm text-slate-900">
                                    {field.value}
                                  </p>
                                )
                              ) : (
                                <p className="text-sm text-slate-400">
                                  Not provided
                                </p>
                              )}
                            </div>

                            <div className="flex justify-end gap-2">
                              {isEditing ? (
                                <>
                                  <button
                                    disabled={savingKey === key}
                                    onClick={() =>
                                      field.isPreset
                                        ? handleSavePreset(field)
                                        : handleSaveCustom(field)
                                    }
                                    className="rounded-lg bg-blue-600 px-3 py-2 text-sm font-semibold text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-40"
                                  >
                                    Save
                                  </button>

                                  <button
                                    disabled={savingKey === key}
                                    onClick={cancelEditing}
                                    className="rounded-lg border border-slate-200 px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50"
                                  >
                                    Cancel
                                  </button>
                                </>
                              ) : (
                                <>
                                  <button
                                    onClick={() => startEditing(field)}
                                    className="rounded-lg border border-slate-200 px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50"
                                  >
                                    Edit
                                  </button>

                                  {!field.isPreset && (
                                    <button
                                      disabled={savingKey === key}
                                      onClick={() => handleDeleteCustom(field)}
                                      className="rounded-lg border border-red-200 px-3 py-2 text-sm font-semibold text-red-700 hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-40"
                                    >
                                      Delete
                                    </button>
                                  )}
                                </>
                              )}
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  )}
                </div>

                {currentFields.length > PAGE_SIZE && (
                  <div className="mt-auto flex shrink-0 items-center justify-between border-t border-slate-100 px-6 py-4">
                    <p className="text-sm text-slate-500">
                      Showing {(page - 1) * PAGE_SIZE + 1}-
                      {Math.min(page * PAGE_SIZE, currentFields.length)} of{' '}
                      {currentFields.length}
                    </p>

                    <div className="flex gap-2">
                      <button
                        disabled={page === 1}
                        onClick={() => setPage((current) => current - 1)}
                        className="rounded-lg border border-slate-200 px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40"
                      >
                        Previous
                      </button>

                      <span className="rounded-lg bg-slate-100 px-3 py-2 text-sm font-medium text-slate-700">
                        Page {page} of {totalPages}
                      </span>

                      <button
                        disabled={page === totalPages}
                        onClick={() => setPage((current) => current + 1)}
                        className="rounded-lg border border-slate-200 px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40"
                      >
                        Next
                      </button>
                    </div>
                  </div>
                )}
              </>
            ) : (
              <div className="p-8 text-sm text-slate-500">
                No profile data available.
              </div>
            )}
          </section>
        </div>
      </div>
    </main>
  );
}
