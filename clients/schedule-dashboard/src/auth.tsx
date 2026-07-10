import React, { JSX } from "react";
import { Navigate, useLocation, useNavigate } from "react-router";

const AppSecretKey = "AppSecret";

/**
 * 登录服务逻辑
 */
const loginService = {
  isAuthenticated: false,
  signin(callback: VoidFunction) {
    loginService.isAuthenticated = true;
    setTimeout(callback, 100);
  },
  signout(callback: VoidFunction) {
    loginService.isAuthenticated = false;
    setTimeout(callback, 100);
  },
};

interface AuthContextType {
  appSecret: string | null;
  signin: (appSecret: string, callback: VoidFunction) => void;
  signout: (callback: VoidFunction) => void;
}

let AuthContext = React.createContext<AuthContextType>(null!);

function AuthProvider({ children }: { children: React.ReactNode }) {
  const initialAppSecret = () => {
    return sessionStorage.getItem(AppSecretKey);
  };

  const [appSecret, setAppSecret] = React.useState<any>(initialAppSecret);

  let signin = (newAppSecret: string, callback: VoidFunction) => {
    return loginService.signin(() => {
      setAppSecret(newAppSecret);
      sessionStorage.setItem(AppSecretKey, newAppSecret);
      callback();
    });
  };

  let signout = (callback: VoidFunction) => {
    return loginService.signout(() => {
      setAppSecret(null);
      sessionStorage.removeItem(AppSecretKey);
      callback();
    });
  };

  let value = { appSecret, signin, signout };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

function AuthStatus() {
  let auth = useAuth();
  let navigate = useNavigate();

  if (!auth.appSecret) {
    return <p>You are not logged in.</p>;
  }

  return (
    <p>
      Welcome {auth.appSecret}!{" "}
      <button
        onClick={() => {
          auth.signout(() => navigate("/"));
        }}
      >
        Sign out
      </button>
    </p>
  );
}

function useAuth() {
  return React.useContext(AuthContext);
}

function RequireAuth({ children }: { children: JSX.Element }) {
  let auth = useAuth();
  let location = useLocation();

  if (!auth.appSecret) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return children;
}

export { loginService, useAuth, AuthProvider, RequireAuth, AuthStatus };
