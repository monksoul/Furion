import { IconCalendarClock } from "@douyinfe/semi-icons";
import { Button, Modal, Tag, Tooltip, Typography } from "@douyinfe/semi-ui";
import Paragraph from "@douyinfe/semi-ui/lib/es/typography/paragraph";
import { useEffect, useState } from "react";
import useFetch from "use-http";
import { Trigger, TriggerTimeline } from "../../types";
import { dayFromNow, dayTime, formatDuration } from "../../utils";
import apiconfig from "../../apiconfig";
import StatusText from "./state-text";
import Timelines from "./timelines";
import { useAuth } from "../../auth";

/**
 * 渲染触发器属性值
 * @param props
 * @returns
 */
export default function RenderValue(props: {
  prop: string;
  value: any;
  trigger: Trigger;
}) {
  const { prop, value, trigger } = props;
  const [visible, setVisible] = useState(false);

  const showDialog = () => {
    setVisible(true);
  };
  const handleOk = () => {
    setVisible(false);
  };
  const handleCancel = () => {
    setVisible(false);
  };

  /**
   * 构建预览节点
   */
  let preview = (
    <span style={{ wordBreak: "break-all" }}>{value?.toString() || ""}</span>
  );

  /**
   * 处理下一次运行时间
   */
  if (prop === "nextRunTime") {
    preview = value ? (
      <Tag color="light-green" type="solid">
        {dayTime(value).format("YYYY/MM/DD HH:mm:ss")} ({dayFromNow(value)})
      </Tag>
    ) : (
      <span></span>
    );
  } else if (prop === "lastRunTime") {
    /**
     * 处理最近运行时间
     */
    preview = value ? (
      <>
        <Tag color="grey" type="light" style={{ verticalAlign: "middle" }}>
          {dayTime(value).format("YYYY/MM/DD HH:mm:ss")} ({dayFromNow(value)})
        </Tag>
        <div>
          <Button
            size="small"
            icon={<IconCalendarClock />}
            style={{ verticalAlign: "middle", fontSize: 12, marginTop: 5 }}
            onClick={showDialog}
          >
            记录
          </Button>
        </div>
      </>
    ) : (
      <span></span>
    );
  } else if (prop === "numberOfRuns") {
    /**
     * 处理运行次数
     */
    preview = (
      <>
        <Tag color="green" type="light">
          {value}
        </Tag>
      </>
    );
  } else if (prop === "numberOfErrors") {
    /**
     * 处理错误次数
     */
    preview = (
      <Tag color="red" type="ghost">
        {value}
      </Tag>
    );
  } else if (prop === "status") {
    /**
     * 处理状态
     */
    preview = (
      <StatusText value={Number(value)} showError onErrorClick={showDialog} />
    );
  } else if (
    prop === "startNow" ||
    prop === "runOnStart" ||
    prop === "resetOnlyOnce"
  ) {
    /**
     * 处理 bool 类型
     */
    preview = (
      <Tag color="blue" type="ghost">
        {value === true ? "是" : "否"}
      </Tag>
    );
  } else if (prop === "triggerId" || prop === "jobId") {
    /**
     * 处理触发器和作业 Id
     */
    preview = (
      <Paragraph
        copyable
        underline
        strong
        style={{ display: "inline-block", wordBreak: "break-all" }}
      >
        {value?.toString() || ""}
      </Paragraph>
    );
  } else if (prop === "args") {
    /**
     * 处理参数类型
     */
    preview = value ? (
      <Paragraph
        copyable
        mark
        style={{ display: "inline-block", wordBreak: "break-all" }}
      >
        {value?.toString() || ""}
      </Paragraph>
    ) : (
      <span></span>
    );
  } else if (prop === "description") {
    /**
     * 处理描述
     */
    const text: string = value || "";
    preview = (
      <Tooltip content={value}>
        {text.length >= 14 ? text.substring(0, 14) + "..." : text}
      </Tooltip>
    );
  } else if (prop === "triggerType") {
    /**
     * 处理触发器类型
     */
    preview = (
      <span style={{ wordBreak: "break-all" }}>{value?.toString() || ""}</span>
    );
  } else if (prop === "result") {
    /**
     * 处理返回结果
     */
    preview = (
      <Typography.Paragraph
        ellipsis={{
          rows: 1,
          expandable: true,
          collapsible: true,
          collapseText: "折叠",
        }}
        style={{ width: 200, wordBreak: "break-all" }}
        copyable
      >
        {value?.toString() || ""}
      </Typography.Paragraph>
    );
  } else if (prop === "elapsedTime") {
    /**
     * 处理运行耗时
     */
    preview = (
      <>
        <Tooltip content={<>{value}ms</>}>
          <span>
            <Tag color="lime" type="light">
              {formatDuration(Number(value))}
            </Tag>
          </span>
        </Tooltip>
      </>
    );
  } else if (
    prop === "updatedTime" ||
    prop === "startTime" ||
    prop === "endTime"
  ) {
    /**
     * 处理其他时间
     */
    preview = value ? (
      <>
        {dayTime(value).format("YYYY/MM/DD HH:mm:ss")} ({dayFromNow(value)})
      </>
    ) : (
      <span></span>
    );
  } else
    preview = (
      <span style={{ wordBreak: "break-all" }}>{value?.toString() || ""}</span>
    );

  return (
    <>
      {preview}
      <LogPanel
        trigger={trigger}
        visible={visible}
        handleOk={handleOk}
        handleCancel={handleCancel}
      />
    </>
  );
}

/**
 * 执行记录
 * @param props
 * @returns
 */
function LogPanel(props: {
  trigger: Trigger;
  visible: boolean;
  handleOk: VoidFunction;
  handleCancel: VoidFunction;
}) {
  const auth = useAuth();
  const { trigger, visible, handleOk, handleCancel } = props;
  const [timelines, setTimelines] = useState<TriggerTimeline[]>([]);

  /**
   * 初始化请求配置
   */
  const { post, response } = useFetch(apiconfig.hostAddress, {
    ...apiconfig.options,
    headers: { ...apiconfig.options.headers, Authorization: auth.appSecret },
  });

  /**
   * 操作作业触发器
   */
  const getTimelines = async (jobid: string, triggerid: string) => {
    const data = await post(
      "/operate-trigger?jobid=" +
        jobid +
        "&triggerid=" +
        triggerid +
        "&action=timelines",
    );

    if (response.ok) setTimelines(data);
  };

  /**
   * 初始化
   */
  useEffect(() => {
    if (visible) {
      getTimelines(trigger.jobId!, trigger.triggerId!);
    }
  }, [visible, trigger.updatedTime]);

  return (
    <Modal
      title={
        <span>
          <Tag size="large" color="green" type="light">
            {trigger.triggerId}
          </Tag>{" "}
          最近运行记录 <StatusText value={Number(trigger.status)} />
        </span>
      }
      visible={visible}
      onOk={handleOk}
      onCancel={handleCancel}
      closeOnEsc={true}
      zIndex={10000000000}
      width={850}
      footer={
        <Button onClick={handleOk} type="tertiary" theme="light">
          关闭
        </Button>
      }
    >
      <Timelines trigger={trigger} timelines={timelines} />
    </Modal>
  );
}
