import { render, screen, fireEvent } from "@testing-library/react";
import { AuthProvider, useAuth } from "@/context/AuthContext";


function TestComponent() {
    const { token, role, userName, email, userId, login, logout } = useAuth(); // use the useAuth hook to get the auth context values

    return (
        <div>
            <p data-testid="token">Token:{token}</p>
            <p data-testid="role">Role:{role}</p>
            <p data-testid="username">UserName:{userName}</p>
            <p data-testid="email">Email:{email}</p>
            <p data-testid="userid">UserId:{userId}</p>
            <button onClick={() => login("new-token", "new-user", "new-user@example.com", "new-user-id", "new-role")}>Login</button>
            <button onClick={logout}>Logout</button>
        </div>
    )
}

describe("AuthContext", () => {
    it("provider and useAuth hook should work correctly", () => {
        render(<AuthProvider> <TestComponent /></AuthProvider>)

        fireEvent.click(screen.getByText("Login"));
        expect(screen.getByTestId("token")).toHaveTextContent("Token:new-token")
        expect(screen.getByTestId("role")).toHaveTextContent("Role:new-role")
        expect(screen.getByTestId("username")).toHaveTextContent("UserName:new-user")
        expect(screen.getByTestId("email")).toHaveTextContent("Email:new-user@example.com")
        expect(screen.getByTestId("userid")).toHaveTextContent("UserId:new-user-id")
    });

    it("logout function should clear the auth state", () => {
        render(<AuthProvider> <TestComponent /></AuthProvider>)

        fireEvent.click(screen.getByText("Login"));
        fireEvent.click(screen.getByText("Logout"));
        expect(screen.getByTestId("token")).toHaveTextContent("Token:")
        expect(screen.getByTestId("role")).toHaveTextContent("Role:")
        expect(screen.getByTestId("username")).toHaveTextContent("UserName:")
        expect(screen.getByTestId("email")).toHaveTextContent("Email:")
        expect(screen.getByTestId("userid")).toHaveTextContent("UserId:")
    });
})