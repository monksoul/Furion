import {
  IconCode,
  IconDelete,
  IconMore,
  IconPlayCircle,
  IconStop,
  IconVigoLogo,
} from "@douyinfe/semi-icons";
import {
  Descriptions,
  Divider,
  Dropdown,
  Popconfirm,
  Popover,
  Tag,
  Toast,
  Tooltip,
  Typography,
} from "@douyinfe/semi-ui";
import { Data } from "@douyinfe/semi-ui/lib/es/descriptions";
import { ColumnProps } from "@douyinfe/semi-ui/lib/es/table/interface";
import Paragraph from "@douyinfe/semi-ui/lib/es/typography/paragraph";
import useFetch from "use-http";
import { JobDetail, Trigger } from "../../types";
import {
  dayFromNow,
  dayTime,
  findMaxUtcTimeString,
  findMinUtcTimeString,
} from "../../utils";
import apiconfig from "../../apiconfig";
import RenderValue from "./render-value";
import StatusText from "./state-text";
import FlipClockCountdown from "@leenguyen/react-flip-clock-countdown";
import { useAuth } from "../../auth";

const style = {
  padding: "10px",
};

const showProps = [
  "triggerId",
  "description",
  "status",
  "lastRunTime",
  "nextRunTime",
  "numberOfRuns",
  "elapsedTime",
];

/**
 * 获取触发器简要渲染数据
 * @param trigger - 触发器对象
 * @returns Descriptions 组件所需的数据数组
 */
function getData(trigger: Trigger): Data[] {
  const data: Data[] = [];
  for (const prop of showProps) {
    const value = trigger[prop as keyof Trigger];
    data.push({
      key: prop.charAt(0).toUpperCase() + prop.slice(1),
      value: <RenderValue prop={prop} value={value} trigger={trigger} />,
    });
  }
  return data;
}

function safeToString(value: unknown): string {
  return value == null ? "" : String(value);
}

/**
 * 表格列配置
 */
const columns: ColumnProps<JobDetail>[] = [
  {
    title: "JobId",
    dataIndex: "jobId",
    width: 280,
    fixed: true,
    render: (_, jobDetail) => {
      const triggerCount = jobDetail.triggers?.length || 0;
      const allStarted =
        triggerCount > 0 && jobDetail.triggers?.every((t) => t.status === 3);

      return (
        <>
          <Popover
            content={
              <div>
                {jobDetail.description && (
                  <>
                    <div
                      style={{
                        padding: "0 8px 10px 8px",
                        textAlign: "center",
                        fontWeight: 500,
                        fontSize: 15,
                      }}
                    >
                      {jobDetail.description}
                    </div>
                    <Divider />
                  </>
                )}
                <div style={style}>
                  {triggerCount === 0 && "暂无触发器"}
                  {jobDetail.triggers?.map((t, i) => {
                    const triggerCount = jobDetail.triggers?.length || 0;
                    return (
                      <div key={t.triggerId}>
                        <div
                          style={{ display: "flex", justifyContent: "center" }}
                        >
                          <FlipClockCountdown
                            to={t.nextRunTime || null!}
                            labels={["天", "时", "分", "秒"]}
                            labelStyle={{
                              fontSize: 12,
                              fontWeight: 500,
                              color: "var(--semi-color-text-0)",
                            }}
                            digitBlockStyle={{
                              width: 20,
                              height: 30,
                              fontSize: 15,
                            }}
                            hideOnComplete={false}
                          />
                        </div>
                        <Descriptions data={getData(t)} />
                        {i < triggerCount - 1 && (
                          <Divider margin="8px" style={{ marginBottom: 16 }} />
                        )}
                      </div>
                    );
                  })}
                </div>
              </div>
            }
            position="right"
            showArrow
          >
            <Paragraph
              copyable
              underline
              strong
              style={{ display: "inline-block" }}
            >
              {jobDetail.jobId}
            </Paragraph>
            <Typography.Text type="secondary" style={{ marginLeft: 5 }}>
              ({triggerCount})
            </Typography.Text>
          </Popover>

          {allStarted && (
            <span style={{ marginLeft: 5 }}>
              <StatusText value={3} />
            </span>
          )}

          {jobDetail.temporary === true && (
            <Tooltip content="执行完毕后自动删除">
              <span>
                <Tag size="small" shape="circle" color="amber">
                  临时
                </Tag>
              </span>
            </Tooltip>
          )}
        </>
      );
    },
  },
  {
    title: "GroupName",
    dataIndex: "groupName",
    width: 150,
  },
  {
    title: "Description",
    dataIndex: "description",
    width: 250,
    render: (text) => {
      const value = safeToString(text);
      return (
        <Tooltip content={text || undefined}>
          {value.length >= 14 ? `${value.substring(0, 14)}...` : value}
        </Tooltip>
      );
    },
  },
  {
    title: "JobType",
    dataIndex: "jobType",
    width: 260,
  },
  {
    title: "AssemblyName",
    dataIndex: "assemblyName",
    width: 160,
  },
  {
    title: "Concurrent",
    dataIndex: "concurrent",
    align: "center",
    width: 120,
    render: (_, jobDetail) => {
      const isParallel = jobDetail.concurrent === true;
      return (
        <Tooltip
          content={
            isParallel
              ? "任务会按触发顺序立即执行，不会等待前一个任务完成。"
              : "若前一个任务尚未完成，则当前任务将进入阻塞状态，并在下一个触发时间点尝试执行。"
          }
        >
          <span>
            <Tag color="red" type={isParallel ? "light" : "solid"}>
              {isParallel ? "并行" : "串行"}
            </Tag>
          </span>
        </Tooltip>
      );
    },
  },
  {
    title: "IncludeAnnotations",
    dataIndex: "includeAnnotations",
    width: 180,
    align: "center",
    render: (_, jobDetail) => {
      const included = jobDetail.includeAnnotations === true;
      return (
        <Tag color="blue" type={included ? "light" : "ghost"}>
          {included ? "是" : "否"}
        </Tag>
      );
    },
  },
  {
    title: "Properties",
    dataIndex: "properties",
    width: 200,
    render: (text) => {
      return (
        <Typography.Paragraph
          ellipsis={{
            rows: 1,
            expandable: true,
            collapsible: true,
            collapseText: "折叠",
            onExpand: (expand, e) => {
              e.stopPropagation();
            },
          }}
          style={{ width: 200 }}
          copyable
        >
          {safeToString(text)}
        </Typography.Paragraph>
      );
    },
  },
  {
    title: "UpdatedTime",
    dataIndex: "updatedTime",
    width: 180,
    render: (text) => {
      return text ? dayTime(text).format("YYYY/MM/DD HH:mm:ss") : "";
    },
  },
  {
    title: "LastRunTime",
    dataIndex: "lastRunTime",
    width: 230,
    fixed: "right",
    resize: false,
    render: (_, jobDetail) => {
      const lastRunTimes =
        jobDetail.triggers
          ?.filter((u) => u.lastRunTime)
          .map((u) => u.lastRunTime!) || [];

      const lastRunTime =
        lastRunTimes.length > 0 ? findMaxUtcTimeString(lastRunTimes) : null;

      return lastRunTime ? (
        <Tag color="grey" type="light" style={{ verticalAlign: "middle" }}>
          {dayTime(lastRunTime).format("YYYY/MM/DD HH:mm:ss")} (
          {dayFromNow(lastRunTime)})
        </Tag>
      ) : null;
    },
  },
  {
    title: "NextRunTime",
    dataIndex: "nextRunTime",
    width: 230,
    fixed: "right",
    resize: false,
    render: (_, jobDetail) => {
      const nextRunTimes =
        jobDetail.triggers
          ?.filter((u) => u.nextRunTime)
          .map((u) => u.nextRunTime!) || [];

      const nextRunTime =
        nextRunTimes.length > 0 ? findMinUtcTimeString(nextRunTimes) : null;

      return nextRunTime ? (
        <Tag
          color="light-green"
          type="solid"
          style={{ verticalAlign: "middle" }}
        >
          {dayTime(nextRunTime).format("YYYY/MM/DD HH:mm:ss")} (
          {dayFromNow(nextRunTime)})
        </Tag>
      ) : null;
    },
  },
  {
    title: "",
    dataIndex: "operate",
    width: 50,
    fixed: "right",
    resize: false,
    render: (_, jobDetail) => (
      <Operation
        jobid={jobDetail.jobId}
        hasTrigger={(jobDetail.triggers?.length || 0) > 0}
        jobDetail={jobDetail}
      />
    ),
    onCell: () => ({
      onClick: (e: React.MouseEvent) => {
        e.stopPropagation();
      },
    }),
  },
];

function reshapeJobDetail(data: JobDetail) {
  if (!data) return { jobDetail: {}, triggers: [] };

  const { triggers, refreshDate, temporary, ...jobDetailRest } = data;

  return {
    jobDetail: jobDetailRest,
    triggers: triggers ?? [],
  };
}

/**
 * 操作按钮组件
 * @param props - 组件参数
 */
function Operation(props: {
  jobid?: string | null;
  hasTrigger: boolean;
  jobDetail: JobDetail;
}) {
  const auth = useAuth();
  const { jobid, hasTrigger, jobDetail } = props;

  /**
   * 初始化请求配置
   */
  const { post, response, loading } = useFetch(apiconfig.hostAddress, {
    ...apiconfig.options,
    headers: { ...apiconfig.options.headers, Authorization: auth.appSecret },
  });

  /**
   * 操作作业
   */
  const callAction = async (action: string) => {
    if (!jobid) {
      Toast.error({ content: "作业 ID 无效", duration: 3 });
      return;
    }

    try {
      const params = new URLSearchParams({ jobid, action });
      await post(`/operate-job?${params.toString()}`);

      if (response.ok) {
        Toast.success({ content: "操作成功", duration: 3 });
      } else {
        throw new Error(response.statusText || "请求失败");
      }
    } catch (error: any) {
      console.error("操作失败:", error);
      Toast.error({ content: error.message || "操作失败", duration: 3 });
    }
  };

  async function copyToClipboard(text: string) {
    try {
      await navigator.clipboard.writeText(text);
      Toast.success({ content: "复制成功", duration: 3 });
      return true;
    } catch (err) {
      Toast.error({ content: "复制失败", duration: 3 });
      return false;
    }
  }

  return (
    <Dropdown
      render={
        <Dropdown.Menu>
          <Dropdown.Item
            onClick={() => callAction("start")}
            disabled={!hasTrigger || loading}
          >
            <IconPlayCircle size="extra-large" /> 启动
          </Dropdown.Item>
          <Dropdown.Item
            onClick={() => callAction("pause")}
            disabled={!hasTrigger || loading}
          >
            <IconStop size="extra-large" /> 暂停
          </Dropdown.Item>
          <Dropdown.Item>
            <Popconfirm
              zIndex={10000000}
              title={`确定要删除当前作业 [${safeToString(jobid)}]？`}
              onConfirm={() => callAction("remove")}
            >
              <IconDelete size="small" /> &nbsp;删除
            </Popconfirm>
          </Dropdown.Item>
          <Dropdown.Item
            onClick={() => callAction("run")}
            disabled={!hasTrigger || loading}
          >
            <IconVigoLogo size="extra-large" /> 手动执行
          </Dropdown.Item>
          <Dropdown.Item
            onClick={() =>
              copyToClipboard(
                JSON.stringify(reshapeJobDetail(jobDetail), null, 2),
              )
            }
            disabled={loading}
          >
            <IconCode size="extra-large" /> 复制 JSON
          </Dropdown.Item>
        </Dropdown.Menu>
      }
    >
      <IconMore
        style={{
          cursor: loading ? "not-allowed" : "pointer",
          color: loading ? "var(--semi-color-disabled-text)" : undefined,
        }}
      />
    </Dropdown>
  );
}

export default columns;
