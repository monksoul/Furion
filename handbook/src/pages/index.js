import Link from "@docusaurus/Link";
import useBaseUrl from "@docusaurus/useBaseUrl";
import useDocusaurusContext from "@docusaurus/useDocusaurusContext";
import useThemeContext from "@theme/hooks/useThemeContext";
import Layout from "@theme/Layout";
import components from "@theme/MDXComponents";
import React from "react";
import contributors from "../data/contributors";
import urls from "../data/urls";
import DockerIcon from "./docker.svg";
import "./index.css";
import "./index.own.css";
import KubernetesIcon from "./kubernetes.svg";
import LinuxIcon from "./linux.svg";
import MacOSIcon from "./macos.svg";
import WindowIcon from "./windows.svg";

function Home() {
  const context = useDocusaurusContext();
  const { siteConfig = {} } = context;
  return (
    <Layout
      title={`让 .NET 开发更简单，更通用，更流行。 ${siteConfig.title}`}
      description="让 .NET 开发更简单，更通用，更流行。"
    >
      <Banner />
      <Gitee />
      <ProccessOn />
      <Remark />
      <WhoUse />
      <Contributor />
      <Links />
    </Layout>
  );
}

function Banner() {
  return (
    <div className="furion-banner">
      <div className="furion-banner-container">
        <div className="furion-banner-item">
          <div className="furion-banner-project">
            Furion{" "}
            <span
              style={{ fontSize: 14, fontWeight: "normal", color: "#8759ff" }}
            >
              [fu:rɪən]
            </span>
          </div>
          <div className="furion-banner-description">
            让 .NET 开发更简单，更通用，更流行。
          </div>
          <ul className="furion-banner-spec">
            <li>基于 .NET5/6 平台，没有历史包袱</li>
            <li>极少依赖，只依赖两个第三方包</li>
            <li>极速上手，一个 Inject() 完成配置</li>
            <li>代码无侵入性，100% 兼容原生写法</li>
          </ul>
          <div className="furion-support-platform">受支持平台：</div>
          <div className="furion-support-icons">
            <span>
              <WindowIcon height="39" width="39" />
            </span>
            <span>
              <LinuxIcon height="39" width="39" />
            </span>
            <span>
              <MacOSIcon height="39" width="39" />
            </span>
            <span>
              <DockerIcon height="39" width="39" />
            </span>
            <span>
              <KubernetesIcon height="39" width="39" />
            </span>
          </div>
          <div className="furion-get-start-btn">
            <Link
              className="furion-get-start"
              to={useBaseUrl("docs/get-start")}
            >
              一分钟上手
            </Link>
            <div className="furion-version">v2.20.2</div>
          </div>
        </div>
        <div className="furion-banner-item">
          <SystemWindow style={{ float: "right" }}>
            <CodeSection
              language="cs"
              // section="schema"
              source={`
public class FurionAppService : IDynamicApiController
{
    private readonly IRepository<User> _userRepository;
    public AppService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    [IfException(1000, "用户ID: {0} 不存在")]
    public async Task<UserDto> GetUser([Range(1, int.MaxValue)] int userId)
    {
        var user = await _userRepository.FindOrDefaultAsync(userId);
        _ = user ?? throw Oops.Oh(1000, userId);
        return user.Adapt<UserDto>();
    }

    public async Task<RemoteData> GetRemote(string id)
    {
        var data = await $"https://www.furion.pro/data?id={id}".GetAsAsync<RemoteData>();
        return data;
    }
}
`}
            />
          </SystemWindow>
        </div>
      </div>
    </div>
  );
}

function Gitee() {
  const { isDarkTheme, setLightTheme, setDarkTheme } = useThemeContext();
  return (
    <div className="furion-content">
      <p className={"furion-small-title" + (isDarkTheme ? " dark" : "")}>
        完全开源免费
      </p>
      <h1 className={"furion-big-title" + (isDarkTheme ? " dark" : "")}>
        ⭐️ 木兰宽松开源协议，代码在 Gitee 平台托管 ⭐️
      </h1>
      <div className="furion-gitee-log">
        <div
          className="furion-log-item"
          style={{ border: "6px solid #723cff" }}
        >
          <div
            className={"furion-log-jiao" + (isDarkTheme ? " dark" : "")}
          ></div>
          <div className="furion-log-number">
            <div style={{ color: "#723cff" }}>7300 +</div>
            <span className={isDarkTheme ? " dark" : ""}>Stars</span>
          </div>
        </div>
        <div
          className="furion-log-item"
          style={{ border: "6px solid #3fbbfe" }}
        >
          <div
            className={"furion-log-jiao" + (isDarkTheme ? " dark" : "")}
          ></div>
          <div className="furion-log-number">
            <div style={{ color: "#3fbbfe" }}>3100 +</div>
            <span className={isDarkTheme ? " dark" : ""}>Forks</span>
          </div>
        </div>
        <div
          className="furion-log-item"
          style={{ border: "6px solid #1fd898" }}
        >
          <div
            className={"furion-log-jiao" + (isDarkTheme ? " dark" : "")}
          ></div>
          <div className="furion-log-number">
            <div style={{ color: "#1fd898" }}>1,246,527</div>
            <span className={isDarkTheme ? " dark" : ""}>Downloads</span>
          </div>
        </div>
      </div>
    </div>
  );
}

function Remark() {
  const { isDarkTheme, setLightTheme, setDarkTheme } = useThemeContext();
  return (
    <div className="furion-content">
      <p className={"furion-small-title" + (isDarkTheme ? " dark" : "")}>
        大量使用者测评
      </p>
      <h1 className={"furion-big-title" + (isDarkTheme ? " dark" : "")}>
        来听听他们是怎么说的
      </h1>
      <div className="furion-remark">
        <div className="furion-remark-item">
          <div className="furion-remark-p">
            <h1>理想和现实</h1>
            <p className={isDarkTheme ? " dark" : undefined}>
              作者的技术是我接触.NET程序员中最好的一个，代码的质量，文档的质量都是一等一的。
            </p>
          </div>
          <div className="furion-remark-p">
            <h1>kesshei</h1>
            <p className={isDarkTheme ? " dark" : undefined}>
              强烈支持，因为有你们，.net 会走的更远。
            </p>
          </div>
          <div className="furion-remark-p">
            <h1>逆天的蝈蝈</h1>
            <p className={isDarkTheme ? " dark" : undefined}>
              非常优秀的开源作品，点赞支持
            </p>
          </div>
        </div>
        <div className="furion-remark-item">
          <div className="furion-remark-p">
            <h1>张芸溪</h1>
            <p className={isDarkTheme ? " dark" : undefined}>
              大概看了一下，觉得项目非常棒。core生态一定能火
            </p>
          </div>
          <div className="furion-remark-p">
            <h1>赖皮小鳄鱼</h1>
            <p className={isDarkTheme ? " dark" : undefined}>
              作者好棒，支持国内.net！
            </p>
          </div>
          <div className="furion-remark-p">
            <h1>gudufy</h1>
            <p className={isDarkTheme ? " dark" : undefined}>
              做得非常棒，打算基于你的框架做一个基本的后台管理出来，供大家快速开发中小型项目。
            </p>
          </div>
        </div>
        <div className="furion-remark-item">
          <div className="furion-remark-p">
            <h1>ThinkCoder</h1>
            <p className={isDarkTheme ? " dark" : undefined}>
              文档写的真不错，代码质量也非常高，注释非常完善，赞一个。
            </p>
          </div>
          <div className="furion-remark-p">
            <h1>liuina</h1>
            <p className={isDarkTheme ? " dark" : undefined}>
              期待文档写完的那一天，绝对惊艳四座。
            </p>
          </div>
          <div className="furion-remark-p">
            <h1>weiyu.xiao</h1>
            <p className={isDarkTheme ? " dark" : undefined}>
              很不错的项目，对快速搭建健壮的技术架构，帮助业务成功很有帮助。
            </p>
          </div>
        </div>
      </div>
      <div>
        <a
          href="https://gitee.com/dotnetchina/Furion#-%E8%B4%A1%E7%8C%AE%E4%BB%A3%E7%A0%81"
          target="_blank"
          className="furion-get-start"
          style={{ margin: 0 }}
        >
          想看更多评论？
        </a>
      </div>
    </div>
  );
}

function WhoUse() {
  return (
    <div className="furion-whouse">
      <div className="furion-who-custom">
        {/* <h1>等待下一个幸运儿，会是你吗？</h1> */}
        <div className="furion-custom-img">
          <a href="https://gitee.com/dotnetchina" target="_blank">
            <img src={useBaseUrl("img/chinadotnet.png")} id="dotnet-china" />
          </a>
        </div>
        <div className="furion-custom-img">
          <img src={useBaseUrl("img/custom1.png")} height="100" />
        </div>
        <div className="furion-custom-img">
          <img src={useBaseUrl("img/custom2.png")} height="100" />
        </div>
        <div className="furion-custom-img">
          <img src={useBaseUrl("img/custom3.png")} height="100" />
        </div>
        <div className="furion-custom-img">
          <img src={useBaseUrl("img/custom4.jpg")} height="100" />
        </div>
        <div className="furion-custom-img">
          <a href="http://www.hezongsoft.net" target="_blank">
            <img src={useBaseUrl("img/custom5.png")} height="100" />
          </a>
        </div>
        <div className="furion-custom-img">
          <a href="http://www.dilon.vip/" target="_blank">
            <img src={useBaseUrl("img/custom6.png")} height="100" />
          </a>
        </div>
        <div className="furion-custom-img">
          <img src={useBaseUrl("img/c10.jpg")} height="100" />
        </div>
        <div className="furion-custom-img">
          <img src={useBaseUrl("img/jjd.png")} height="100" />
        </div>
        <div className="furion-custom-img">
          <a href="http://www.samfeng.cn/" target="_blank">
            <img src={useBaseUrl("img/custom10.png")} height="100" />
          </a>
        </div>
        <div className="furion-custom-img">
          <a href="http://www.ketaisoft.com/" target="_blank">
            <img src={useBaseUrl("img/custom11.png")} height="100" />
          </a>
        </div>
      </div>
      <div className="furion-who-des">
        <div style={{ maxWidth: 350 }}>
          <div></div>
          <h1>他们都在用</h1>
          <p>
            Furion
            已在全球多个项目中使用。从小型企业到企业的解决方案及知名企业。他们在简单软件和复杂管理系统的开发方面都信任我们。
          </p>
          <Link className="furion-get-start" to="mailto:monksoul@outlook.com">
            立即添加您的公司
          </Link>
        </div>
      </div>
    </div>
  );
}

function Links() {
  const { isDarkTheme, setLightTheme, setDarkTheme } = useThemeContext();
  return (
    <div className="furion-links">
      <p className={"furion-small-title" + (isDarkTheme ? " dark" : "")}>
        友情链接
      </p>
      <h1 className={"furion-big-title" + (isDarkTheme ? " dark" : "")}>
        它们一样是优秀的项目/网站
      </h1>
      <div className="furion-links-content">
        {urls.map((item, i) => (
          <a href={item.url} key={i} target="_blank" title={item.title}>
            {item.text}
          </a>
        ))}
      </div>
    </div>
  );
}

function Contributor() {
  const { isDarkTheme, setLightTheme, setDarkTheme } = useThemeContext();

  return (
    <div className="furion-contributors">
      <p className={"furion-small-title" + (isDarkTheme ? " dark" : "")}>
        框架贡献者
      </p>
      <h1 className={"furion-big-title" + (isDarkTheme ? " dark" : "")}>
        正是他们成就了 Furion 不凡
      </h1>
      <div className="furion-contributors-list">
        {contributors.map((c, i) => (
          <div
            className={"furion-contributor-item" + (isDarkTheme ? " dark" : "")}
            key={i}
          >
            {!c.extra ? (
              ""
            ) : (
              <div className="furion-contributor-extra">{c.extra}</div>
            )}
            <a
              href={c.link}
              target="_blank"
              title={"点击查看 " + c.author + " 开源主页"}
            >
              <img
                src={c.photo}
                alt={c.author + " 头像"}
                title={"点击查看 " + c.author + " 开源主页"}
              />
              <div>{c.author}</div>
            </a>
          </div>
        ))}
      </div>
    </div>
  );
}

function ProccessOn() {
  const { isDarkTheme, setLightTheme, setDarkTheme } = useThemeContext();
  return (
    <div className="furion-proccesson">
      <p className={"furion-small-title" + (isDarkTheme ? " dark" : "")}>
        功能模块
      </p>
      <h1 className={"furion-big-title" + (isDarkTheme ? " dark" : "")}>
        麻雀虽小五脏俱全
      </h1>
      <div className="furion-proccesson-content">
        <iframe
          id="embed_dom"
          name="embed_dom"
          frameBorder="0"
          style={{ display: "block", width: "100%", height: "600px" }}
          src="https://www.processon.com/embed/60a7489f6376891bafa2d353"
        ></iframe>
      </div>
    </div>
  );
}

function CodeSection(props) {
  let { language, replace, section, source } = props;

  source = source.replace(/\/\/ <.*?\n/g, "");

  if (replace) {
    for (const [pattern, value] of Object.entries(replace)) {
      source = source.replace(new RegExp(pattern, "gs"), value);
    }
  }

  source = source.trim();
  if (!source.includes("\n")) {
    source += "\n";
  }

  return (
    <components.pre>
      <components.code
        children={source}
        className={`language-${language}`}
        mdxType="code"
        originalType="code"
        parentName="pre"
      />
    </components.pre>
  );
}

function SystemWindow(systemWindowProps) {
  const { children, className, ...props } = systemWindowProps;
  return (
    <div
      {...props}
      className={"system-window blue-accent preview-border " + className}
    >
      <div className="system-top-bar">
        <span
          className="system-top-bar-circle"
          style={{ backgroundColor: "#8759ff" }}
        />
        <span
          className="system-top-bar-circle"
          style={{ backgroundColor: "#3fc4fe" }}
        />
        <span
          className="system-top-bar-circle"
          style={{ backgroundColor: "#42ffac" }}
        />
      </div>
      {children}
    </div>
  );
}

export default Home;
