import {
  Popover,
  Tag,
  TextArea,
  Timeline,
  Tooltip,
  Typography,
} from "@douyinfe/semi-ui";
import { Trigger, TriggerTimeline } from "../../types";
import { dayFromNow, dayTime, formatDuration } from "../../utils";
import StatusText from "./state-text";
import { IconActivity, IconUploadError } from "@douyinfe/semi-icons";

export default function Timelines({
  timelines,
  trigger,
}: {
  timelines: TriggerTimeline[];
  trigger?: Trigger;
}) {
  return (
    <Timeline mode="center">
      {timelines.map((timeline, i) => (
        <Timeline.Item
          key={timeline.jobId! + timeline.triggerId! + i}
          time={
            <div style={{ display: "inline-flex" }}>
              {timeline.nextRunTime ? (
                <div style={{ display: "inline-block" }}>
                  <Tag color={"light-green"} type={i === 0 ? "solid" : "light"}>
                    {dayTime(timeline.nextRunTime).format(
                      "YYYY/MM/DD HH:mm:ss",
                    )}
                    ({dayFromNow(timeline.nextRunTime)})
                  </Tag>
                  <div>NextRunTime</div>
                </div>
              ) : (
                <StatusText value={Number(timeline.status)} />
              )}
              <span style={{ padding: "0 3px" }}>{"<"}-</span>
              <div style={{ display: "inline-block" }}>
                <Tag color="grey" type="light">
                  {dayTime(timeline.lastRunTime).format("YYYY/MM/DD HH:mm:ss")}(
                  {dayFromNow(timeline.lastRunTime)})
                </Tag>
                <div>LastRunTime</div>
              </div>
            </div>
          }
          dot={
            i === 0 ? <IconActivity style={{ color: "green" }} /> : undefined
          }
          extra={
            <>
              <span>
                {trigger?.triggerType || ""}: {trigger?.args || ""}
              </span>
              {timeline.result && (
                <div>
                  <Typography.Paragraph
                    ellipsis={{
                      rows: 1,
                      expandable: true,
                      expandText: "展开",
                      collapsible: true,
                      collapseText: "折叠",
                    }}
                    style={{ width: 200 }}
                    copyable
                  >
                    {timeline.result}
                  </Typography.Paragraph>
                </div>
              )}
            </>
          }
        >
          第{" "}
          <Tag color="green" type="light">
            {timeline.numberOfRuns}
          </Tag>{" "}
          次运行，耗时{" "}
          <Tooltip content={<>{timeline.elapsedTime}ms</>} zIndex={10000000001}>
            <span>
              <Tag color="lime" type="light">
                {formatDuration(timeline.elapsedTime!)}
              </Tag>
            </span>
          </Tooltip>{" "}
          {timeline.mode === 1 && (
            <Tag color="yellow" type="solid">
              手动
            </Tag>
          )}
          {timeline.exception && (
            <Popover
              showArrow
              content={
                <div
                  className="exception-box"
                  style={{
                    padding: 10,
                    width: 400,
                  }}
                >
                  <TextArea value={timeline.exception} rows={10} />
                </div>
              }
              trigger="click"
              zIndex={10000000002}
            >
              <IconUploadError
                style={{
                  position: "relative",
                  color: "red",
                  top: 4,
                  cursor: "pointer",
                  marginLeft: 5,
                }}
              />
            </Popover>
          )}
        </Timeline.Item>
      ))}
    </Timeline>
  );
}
