import { useEffect } from "react";
import { useAuth } from "./auth";
import styles from "./login.module.css";
import { Button, Input, Space, Toast, Typography } from "@douyinfe/semi-ui";
import useFetch from "use-http";
import apiconfig from "./apiconfig";
import { useLocation, useNavigate } from "react-router";

export default function Login() {
  /**
   * 初始化请求配置
   */
  const { post, response, error } = useFetch(
    apiconfig.hostAddress,
    apiconfig.options,
  );

  let navigate = useNavigate();
  let location = useLocation();
  let auth = useAuth();

  let from = location.state?.from?.pathname || "/";

  useEffect(() => {
    const theme = localStorage.getItem("theme-mode");
    if (theme === "dark") {
      document.body.setAttribute("theme-mode", "dark");
    } else {
      document.body.removeAttribute("theme-mode");
    }
  }, []);

  /**
   * 操作作业触发器
   */
  const loginHandle = async (username: string, password: string) => {
    const formData = new FormData();
    formData.append("username", username);
    formData.append("password", password);

    const data = await post("/login", formData);
    if (response.ok) {
      Toast.success({ content: "登录成功", stack: true });
      var appSecret = response.headers.get("access-token");

      auth.signin(appSecret!, () => {
        navigate(from, { replace: true });
      });
    } else {
      Toast.error({
        content: data || "服务器异常",
        stack: true,
      });
    }
  };

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    let formData = new FormData(event.currentTarget);
    let username = formData.get("username") as string;
    let password = formData.get("password") as string;

    if (username?.trim().length === 0) {
      Toast.error({ content: "请输入用户名" });
      return;
    }

    await loginHandle(username?.trim(), password?.trim());
  }

  return (
    <div className={styles.main}>
      <div className={styles.login}>
        <div className={styles.component66}>
          <div className={styles.header}>
            <div className={styles.title}>{apiconfig.title}</div>
          </div>
        </div>
        <div className={styles.form}>
          <form onSubmit={handleSubmit} className={styles.inputs}>
            <Space vertical align="start">
              <Typography.Text strong>用户名</Typography.Text>
              <Input
                name="username"
                placeholder="输入用户名"
                style={{ width: 300 }}
                className={styles.formField}
                defaultValue={apiconfig.loginConfig?.defaultUsername || ""}
              />
            </Space>
            <Space vertical align="start">
              <Typography.Text strong>密码</Typography.Text>
              <Input
                mode="password"
                name="password"
                placeholder="输入密码"
                style={{ width: 300 }}
                className={styles.formField}
                defaultValue={apiconfig.loginConfig?.defaultPassword || ""}
              />
            </Space>
            <Button theme="solid" className={styles.button} htmlType="submit">
              登录
            </Button>
          </form>
        </div>
      </div>
    </div>
  );
}
