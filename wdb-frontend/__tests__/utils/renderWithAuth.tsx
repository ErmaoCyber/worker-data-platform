import ProfilePage from "@/app/worker/profile/page";
import { AuthContext } from "@/context/AuthContext";
import { render } from "@testing-library/react";
import { JSX } from "react/jsx-dev-runtime";

//mock the authstate type .
type AuthState = {
    token?: string | null;
    role?: string | null;
    userName?: string | null;
    email?: string | null;
    userId?: string | null;
};


// 
export function renderWithAuth(ui: JSX.Element, authState: AuthState = {}) {
    const mockValue = {
        token: authState.token || "mock-token",
        role: authState.role || "worker",
        userName: authState.userName || "mock-user",
        email: authState.email || "mock-user@example.com",
        userId: authState.userId || "mock-user-id",
        login: jest.fn(),
        logout: jest.fn(), // mock logout function
    }

    return render(
        <AuthContext.Provider value={mockValue}>
            {ui}
        </AuthContext.Provider> // render the provided ui within the mocked value of auth.
    );
}
