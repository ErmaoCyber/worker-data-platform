import { render, screen } from "@testing-library/react";
import WorkerDashboardView from "./WorkerDashboardView";
import type { WorkerDashboardResponse } from "@/lib/workerDashboardApi";

const mockData: WorkerDashboardResponse = {
  worker: {
    id: "11111111-1111-1111-1111-111111111111",
    name: "user",
    email: "user@example.com",
    verified: true,
    blockchainAddress: "0xf39Fd6e51aad88F6F4ce6aB8827279cffFb92266",
  },
  latestRequests: [
    {
      requestId: "55555555-5555-5555-5555-555555555551",
      employerId: "22222222-2222-2222-2222-222222222222",
      employerName: "First Step Solutions",
      createdAt: "2026-04-15T10:00:00Z",
      reason: "Site onboarding",
    },
  ],
  blockchainRecords: [],
  blockchainAvailable: true,
};

const mockDataWithBlockchainRecord: WorkerDashboardResponse = {
  ...mockData,
  blockchainRecords: [
    {
      employerAddress: "0x70997970C51812dc3A010C7d01b50e0d17dc79C8",
      workerAddress: "0xf39Fd6e51aad88F6F4ce6aB8827279cffFb92266",
      action: "PermissionApproved",
      txHash:
        "0x4c8201b58350618d420f27129a6a8fdb27957d23850303c874e07adbebe64cc8",
      date: "2026-05-05T23:56:13Z",
    },
  ],
};

describe("WorkerDashboardView", () => {
  it("renders worker basic information", () => {
    render(<WorkerDashboardView data={mockData} />);

    expect(screen.getByText("Worker Dashboard")).toBeInTheDocument();
    expect(screen.getByText("user")).toBeInTheDocument();
    expect(screen.getByText("user@example.com")).toBeInTheDocument();
    expect(screen.getByText("Verified")).toBeInTheDocument();
  });

  it("renders latest requests", () => {
    render(<WorkerDashboardView data={mockData} />);

    expect(screen.getByText("Latest Requests")).toBeInTheDocument();
    expect(screen.getByText("First Step Solutions")).toBeInTheDocument();
    expect(screen.getByText("Site onboarding")).toBeInTheDocument();
  });

  it("renders empty blockchain placeholder", () => {
    render(<WorkerDashboardView data={mockData} />);

    expect(
      screen.getByText("No blockchain records available yet.")
    ).toBeInTheDocument();
  });

  it("renders blockchain records", () => {
    render(<WorkerDashboardView data={mockDataWithBlockchainRecord} />);

    expect(screen.getByText("Blockchain Records")).toBeInTheDocument();
    expect(screen.getByText("Connected")).toBeInTheDocument();
    expect(screen.getByText("Permission approved")).toBeInTheDocument();
    expect(screen.getByText("On-chain")).toBeInTheDocument();
    expect(screen.getByText("0x7099...79C8")).toBeInTheDocument();
    expect(screen.getByText("0xf39F...2266")).toBeInTheDocument();
    expect(screen.getByText("0x4c8201b5...64cc8")).toBeInTheDocument();
  });
});
