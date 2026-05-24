"use client";

import { useState } from "react";
import { FetchApi } from "../../lib/api";
import ConfirmModal from "./ConfirmModal";

export interface Field {
  id: string;
  label: string;
  checked: boolean;
}

export interface Row {
  id: string;
  company: string;
  date: string;
  fields: Field[];
  reason: string;
  onComplete: () => void;
  expiryDate: string;
}

export default function RequestRow({ company, date, fields, reason, onComplete }: Row) {
  const [checkedFields, setCheckedFields] = useState<Field[]>(fields);
  const [errorMsg, setErrorMsg] = useState("");
  const [pendingAction, setPendingAction] = useState<"approve" | "reject" | "">("");
  const [showModal, setShowModal] = useState(false);
  const [showExpiry, setShowExpiry] = useState(false);
  const [expiryDate, setExpiryDate] = useState("");

  const toggleField = (label: string) => {
    setCheckedFields((prev) =>
      prev.map((field) =>
        field.label === label
          ? { ...field, checked: !field.checked }
          : field
      )
    );
  };

  const onExpiry = (date: string) => {
    setExpiryDate(date);
  };

  async function changePermission(status: "approve" | "reject") {
    const checkedIds = checkedFields
      .filter((field) => field.checked)
      .map((field) => field.id);

    if (status === "approve" && !expiryDate) {
      setErrorMsg("Please select an expiry date before approving.");
      return;
    }

    try {
      await Promise.all(
        checkedIds.map((permissionId) =>
          FetchApi(`/api/Permission/${permissionId}/${status}`, {
            method: "PATCH",
            body:
              status === "approve"
                ? JSON.stringify({ expiryDate })
                : undefined,
          })
        )
      );

      setErrorMsg("");
      onComplete();
    } catch (error) {
      setErrorMsg(`${error}`);
    }
  }

  return (
    <div className="flex justify-between items-center px-5 py-4 border-b border-gray-200">
      <div className="flex flex-col gap-2">
        <p className="text-sm text-gray-900">{company}</p>
        <p className="text-xs text-gray-500">{date}</p>

        <div className="flex gap-3 flex-wrap">
          {checkedFields.map((field) => (
            <label
              key={field.id}
              className="flex items-center gap-2 border border-gray-300 rounded-md px-3 py-1 text-sm text-gray-700 cursor-pointer"
            >
              <input
                type="checkbox"
                checked={field.checked}
                onChange={() => toggleField(field.label)}
                className="cursor-pointer"
              />
              {field.label}
            </label>
          ))}
        </div>

        <div className="flex items-center gap-1 text-xs text-gray-500">
          <span>{reason}</span>
        </div>

        {errorMsg && <p className="text-sm text-red-500">{errorMsg}</p>}
      </div>

      <div className="flex gap-2">
        <button
          className="bg-green-500 hover:bg-green-600 text-white rounded-md px-4 py-2 text-base disabled:opacity-40 disabled:cursor-not-allowed cursor-pointer transition-colors"
          disabled={checkedFields.filter((field) => field.checked).length === 0}
          onClick={() => {
            setShowModal(true);
            setPendingAction("approve");
            setShowExpiry(true);
            setErrorMsg("");
          }}
        >
          ✔
        </button>

        <button
          className="bg-red-500 hover:bg-red-600 text-white rounded-md px-4 py-2 text-base disabled:opacity-40 disabled:cursor-not-allowed cursor-pointer transition-colors"
          disabled={checkedFields.filter((field) => field.checked).length === 0}
          onClick={() => {
            setShowModal(true);
            setPendingAction("reject");
            setShowExpiry(false);
            setExpiryDate("");
            setErrorMsg("");
          }}
        >
          ✖
        </button>

        {showModal && (
          <ConfirmModal
            company={company}
            status={pendingAction}
            selectedFields={checkedFields
              .filter((field) => field.checked)
              .map((field) => field.label)}
            onConfirm={() => {
              if (pendingAction) {
                changePermission(pendingAction);
              }
              setShowModal(false);
            }}
            onCancel={() => {
              setShowModal(false);
              setPendingAction("");
              setExpiryDate("");
            }}
            showExpiry={showExpiry}
            choseExpiry={onExpiry}
          />
        )}
      </div>
    </div>
  );
}
