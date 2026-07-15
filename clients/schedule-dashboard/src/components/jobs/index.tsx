import {
  IconCalendar,
  IconCopyAdd,
  IconDelete,
  IconList,
  IconMore,
  IconPlayCircle,
  IconSearch,
  IconSmallTriangleRight,
  IconStop,
  IconUploadError,
  IconVigoLogo,
} from "@douyinfe/semi-icons";
import {
  Button,
  Descriptions,
  Divider,
  Dropdown,
  Input,
  JsonViewer,
  Modal,
  Popconfirm,
  Popover,
  Space,
  Table,
  TabPane,
  Tabs,
  Tag,
  TextArea,
  Toast,
  Tooltip,
  Typography,
} from "@douyinfe/semi-ui";
import { Data } from "@douyinfe/semi-ui/lib/es/descriptions";
import {
  ExpandedRowRender,
  OnRow,
} from "@douyinfe/semi-ui/lib/es/table/interface";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import useFetch from "use-http";
import { JobDetail, Scheduler, TriggerTimeline } from "../../types";
import apiconfig from "../../apiconfig";
import columns from "./columns";
import RenderValue from "./render-value";
import FlipClockCountdown from "@leenguyen/react-flip-clock-countdown";
import { dayFromNow, dayTime, formatDuration } from "../../utils";
import styles from "./index.module.css";
import clsx from "clsx";
import { useAuth } from "../../auth";

const style = {
  boxShadow: "var(--semi-shadow-elevated)",
  backgroundColor: "var(--semi-color-bg-2)",
  borderRadius: "4px",
  padding: "10px",
  margin: "10px",
  width: "380px",
};

function getOValueByData(key: string, expandData: Data[]): any {
  const item = expandData.find((u) => u.key === key) as any;
  return item?.ovalue ?? null;
}

function safeToString(value: any): string {
  return value == null ? "" : String(value);
}

/**
 * 生成唯一作业ID：job_20260513111300345
 */
function generateJobId(prefix: string = "job"): string {
  const now = new Date();
  const pad = (n: number) => String(n).padStart(2, "0");

  const timestamp =
    now.getFullYear() +
    pad(now.getMonth() + 1) +
    pad(now.getDate()) +
    pad(now.getHours()) +
    pad(now.getMinutes()) +
    pad(now.getSeconds()) +
    pad(now.getMilliseconds());

  return `${prefix}_${timestamp}`;
}

function getDefaultJsonValue() {
  let _jobId = generateJobId();

  return `{
  "jobDetail": {
    "jobId": "${_jobId}",
    "groupName": null,
    "jobType": "Furion.Application.TestJob",
    "assemblyName": "Furion.Application",
    "description": null,
    "concurrent": true,
    "includeAnnotations": false,
    "properties": "{}",
    "updatedTime": "2026-01-01 00:00:00.483"
  },
  "triggers": [
    {
      "triggerId": null,
      "jobId": "${_jobId}",
      "triggerType": "Furion.Schedule.PeriodTrigger",
      "assemblyName": "Furion",
      "args": "[5000]",
      "description": null,
      "status": 2,
      "startTime": null,
      "endTime": null,
      "lastRunTime": "2026-01-01 00:00:00.768",
      "nextRunTime": "2026-01-01 17:52:34.769",
      "numberOfRuns": 1,
      "maxNumberOfRuns": 0,
      "numberOfErrors": 0,
      "maxNumberOfErrors": 0,
      "numRetries": 0,
      "retryTimeout": 1000,
      "startNow": true,
      "runOnStart": false,
      "resetOnlyOnce": true,
      "result": null,
      "elapsedTime": 100,
      "updatedTime": "2026-01-01 00:00:00.803"
    }
  ]
}`;
}

let defaultJsonValue = getDefaultJsonValue();

function getNumberFromUrl(key: string, fallback: number): number {
  const params = new URLSearchParams(window.location.search);
  const value = params.get(key);
  if (value) {
    const num = parseInt(value, 10);
    return isNaN(num) ? fallback : num;
  }
  return fallback;
}

function syncUrlParams(
  tab: string,
  page: number,
  pageSize: number,
  search: string,
) {
  const params = new URLSearchParams(window.location.search);

  if (tab && tab !== "list") {
    params.set("tab", tab);
  } else {
    params.delete("tab");
  }

  if (page > 1) {
    params.set("page", String(page));
  } else {
    params.delete("page");
  }

  if (pageSize !== 10) {
    params.set("pageSize", String(pageSize));
  } else {
    params.delete("pageSize");
  }

  if (search) {
    params.set("q", search);
  } else {
    params.delete("q");
  }

  const searchString = params.toString();
  const newUrl =
    window.location.pathname + (searchString ? `?${searchString}` : "");
  window.history.replaceState(null, "", newUrl);
}

export default function Jobs({ mode }: { mode: string }) {
  const auth = useAuth();

  const initialParams = useMemo(
    () => new URLSearchParams(window.location.search),
    [],
  );
  const [activeTab, setActiveTab] = useState(
    () => initialParams.get("tab") || "list",
  );
  const [words, setWords] = useState<string>(
    () => initialParams.get("q") || "",
  );
  const [pagination, setPagination] = useState({
    currentPage: getNumberFromUrl("page", 1),
    pageSize: getNumberFromUrl("pageSize", 10),
    total: 0,
  });

  const [jobs, setJobs] = useState<Scheduler[]>([]);
  const [allTimelines, setAllTimelines] = useState<TriggerTimeline[]>([]);
  const [jsonValue, setJsonValue] = useState(defaultJsonValue);
  const [submitting, setSubmitting] = useState(false);

  const [manualRunModal, setManualRunModal] = useState<{
    visible: boolean;
    jobId: string;
    triggerId: string;
    json: string;
  }>({ visible: false, jobId: "", triggerId: "", json: "" });

  const jsonviewerRef = useRef<JsonViewer>(null!);

  const jobList = useMemo(() => {
    const trimWords = words.trim();
    if (!trimWords) return jobs;

    return jobs.filter((u) => {
      const matchJobDetail = () => {
        const jd = u.jobDetail;
        if (!jd) return false;
        return (
          jd.jobId?.includes(trimWords) ||
          jd.groupName?.includes(trimWords) ||
          jd.description?.includes(trimWords) ||
          jd.jobType?.includes(trimWords) ||
          jd.assemblyName?.includes(trimWords) ||
          jd.properties?.includes(trimWords) ||
          (jd.concurrent ? "并行" : "串行").includes(trimWords) ||
          (jd.temporary ? "临时" : "").includes(trimWords)
        );
      };

      const matchTriggers = () => {
        return (u.triggers || []).some((t) =>
          [
            t.triggerId,
            t.description,
            t.triggerType,
            t.assemblyName,
            t.args,
          ].some((val) => val?.includes(trimWords)),
        );
      };

      return matchJobDetail() || matchTriggers();
    });
  }, [jobs, words]);

  const { post, response } = useFetch(apiconfig.hostAddress, {
    ...apiconfig.options,
    headers: { ...apiconfig.options.headers, Authorization: auth.appSecret },
  });

  const loadJobs = useCallback(async () => {
    try {
      const data = await post("/get-jobs");
      if (response.ok) {
        setJobs(data || []);
      }
    } catch (error) {
      Toast.error({ content: "加载作业列表失败", duration: 3 });
    }
  }, [post, response]);

  const loadAllTimelines = useCallback(async () => {
    try {
      const data = await post("/timelines-log");
      if (response.ok) {
        setAllTimelines(data || []);
      }
    } catch (error) {
      Toast.error({ content: "加载运行记录失败", duration: 3 });
    }
  }, [post, response]);

  const callAction = useCallback(
    async (jobid: string, triggerid: string, action: string) => {
      try {
        const params = new URLSearchParams({ jobid, triggerid, action });
        await post(`/operate-trigger?${params.toString()}`);

        if (response.ok) {
          Toast.success({ content: "操作成功", duration: 3 });
        } else {
          throw new Error(response.statusText || "请求失败");
        }
      } catch (error: any) {
        Toast.error({ content: error.message || "操作失败", duration: 3 });
      }
    },
    [post, response],
  );

  const handleRunWithCustomData = useCallback(async () => {
    const { jobId, triggerId, json } = manualRunModal;
    if (!jobId || !triggerId) return;

    let customData: object | null = null;
    if (json.trim()) {
      try {
        customData = JSON.parse(json);
      } catch (e) {
        Toast.error({ content: "自定义数据 JSON 格式错误", duration: 3 });
        return;
      }
    }

    try {
      const params = new URLSearchParams({
        jobid: jobId,
        triggerid: triggerId,
        action: "run",
      });

      await post(`/operate-trigger?${params.toString()}`, customData!);

      if (response.ok) {
        Toast.success({ content: "手动执行成功", duration: 3 });
      } else {
        throw new Error(response.statusText || "请求失败");
      }
    } catch (error: any) {
      Toast.error({ content: error.message || "操作失败", duration: 3 });
    } finally {
      setManualRunModal({ visible: false, jobId: "", triggerId: "", json: "" });
    }
  }, [manualRunModal, post, response]);

  const handleSubmitJob = useCallback(async () => {
    if (submitting) return;

    try {
      setSubmitting(true);

      let payload: string;

      try {
        payload = JSON.parse(jsonValue);
      } catch (e) {
        Toast.error({ content: "数据 JSON 格式错误", duration: 3 });
        return;
      }

      const result = await post("/add-job", payload);

      if (response.ok) {
        Toast.success({ content: "作业添加成功", duration: 3 });

        var newJsonValue = getDefaultJsonValue();
        setJsonValue(newJsonValue);
        defaultJsonValue = newJsonValue;
      } else {
        throw new Error(response.statusText || "请求失败");
      }
    } catch (error: any) {
      Toast.error({ content: error.message || "操作失败", duration: 3 });
    } finally {
      setSubmitting(false);
    }
  }, [post, response, jsonValue]);

  const data: JobDetail[] = useMemo(() => {
    if (!jobList?.length) return [];

    return jobList
      .filter((scheduler) => {
        if (
          apiconfig.displayEmptyTriggerJobs === "false" &&
          !scheduler.triggers?.length
        ) {
          return false;
        }
        return true;
      })
      .map((scheduler) => {
        return {
          ...scheduler.jobDetail!,
          triggers: scheduler.triggers,
          refreshDate: new Date(),
        } as JobDetail;
      });
  }, [jobList]);

  const paginatedData = useMemo(() => {
    const start = (pagination.currentPage - 1) * pagination.pageSize;
    const end = start + pagination.pageSize;
    return data.slice(start, end);
  }, [data, pagination.currentPage, pagination.pageSize]);

  useEffect(() => {
    const handlePopState = () => {
      const params = new URLSearchParams(window.location.search);
      setActiveTab(params.get("tab") || "list");
      setWords(params.get("q") || "");
      setPagination((prev) => ({
        ...prev,
        currentPage: getNumberFromUrl("page", 1),
        pageSize: getNumberFromUrl("pageSize", 10),
      }));
    };
    window.addEventListener("popstate", handlePopState);
    return () => window.removeEventListener("popstate", handlePopState);
  }, []);

  useEffect(() => {
    loadJobs();
    loadAllTimelines();

    const eventSource = new EventSource(
      `${apiconfig.hostAddress}/check-change?appsecret=${auth.appSecret}`,
    );

    eventSource.onmessage = () => {
      loadJobs();
      loadAllTimelines();
    };

    eventSource.onerror = (err) => {
      console.warn("EventSource 连接异常:", err);
    };

    return () => {
      eventSource.close();
    };
  }, []);

  const expandRowRender: ExpandedRowRender<JobDetail> = useCallback(
    (jobDetail) => {
      const scheduler = jobList.find(
        (u) => u.jobDetail?.jobId === jobDetail?.jobId,
      );
      if (!scheduler) return null;

      const triggerData: Data[][] =
        scheduler.triggers?.map((trigger) => {
          return Object.entries(trigger).map(
            ([prop, value]) =>
              ({
                key: prop.charAt(0).toUpperCase() + prop.slice(1),
                value: (
                  <RenderValue prop={prop} value={value} trigger={trigger} />
                ),
                ovalue: value,
              }) as Data,
          );
        }) || [];

      return (
        <div style={{ display: "flex", flexWrap: "wrap" }}>
          {triggerData.map((expandData, index) => {
            const triggerId = safeToString(
              getOValueByData("TriggerId", expandData),
            );
            const jobId = safeToString(getOValueByData("JobId", expandData));

            return (
              <div style={style} key={`${jobId}_${triggerId}_${index}`}>
                <div
                  style={{
                    marginTop: 3,
                    marginRight: 5,
                    marginLeft: 5,
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "flex-start",
                  }}
                >
                  {Number(getOValueByData("Status", expandData)) === 3 ? (
                    <Tooltip content="启动">
                      <IconPlayCircle
                        style={{ color: "red", cursor: "pointer" }}
                        size="large"
                        onClick={() =>
                          callAction(
                            safeToString(getOValueByData("JobId", expandData)),
                            safeToString(
                              getOValueByData("TriggerId", expandData),
                            ),
                            "start",
                          )
                        }
                      />
                    </Tooltip>
                  ) : (
                    <span></span>
                  )}

                  <FlipClockCountdown
                    to={getOValueByData("NextRunTime", expandData)}
                    labels={["天", "时", "分", "秒"]}
                    labelStyle={{
                      fontSize: 12,
                      fontWeight: 500,
                      color: "var(--semi-color-text-0)",
                    }}
                    digitBlockStyle={{ width: 20, height: 30, fontSize: 15 }}
                    hideOnComplete={false}
                  />

                  <Dropdown
                    render={
                      <Dropdown.Menu>
                        <Dropdown.Item
                          onClick={() =>
                            callAction(
                              safeToString(
                                getOValueByData("JobId", expandData),
                              ),
                              safeToString(
                                getOValueByData("TriggerId", expandData),
                              ),
                              "start",
                            )
                          }
                        >
                          <IconPlayCircle size="extra-large" /> 启动
                        </Dropdown.Item>
                        <Dropdown.Item
                          onClick={() =>
                            callAction(
                              safeToString(
                                getOValueByData("JobId", expandData),
                              ),
                              safeToString(
                                getOValueByData("TriggerId", expandData),
                              ),
                              "pause",
                            )
                          }
                        >
                          <IconStop size="extra-large" /> 暂停
                        </Dropdown.Item>
                        <Dropdown.Item>
                          <Popconfirm
                            zIndex={10000000}
                            title={`确定要删除当前触发器 [${safeToString(
                              getOValueByData("TriggerId", expandData),
                            )}]？`}
                            onConfirm={() =>
                              callAction(
                                safeToString(
                                  getOValueByData("JobId", expandData),
                                ),
                                safeToString(
                                  getOValueByData("TriggerId", expandData),
                                ),
                                "remove",
                              )
                            }
                          >
                            <IconDelete size="small" /> &nbsp;删除
                          </Popconfirm>
                        </Dropdown.Item>
                        <Dropdown.Item
                          onClick={() => {
                            const jobId = safeToString(
                              getOValueByData("JobId", expandData),
                            );
                            const triggerId = safeToString(
                              getOValueByData("TriggerId", expandData),
                            );
                            setManualRunModal({
                              visible: true,
                              jobId,
                              triggerId,
                              json: "",
                            });
                          }}
                        >
                          <IconVigoLogo size="extra-large" /> 手动执行
                        </Dropdown.Item>
                      </Dropdown.Menu>
                    }
                  >
                    <IconMore style={{ cursor: "pointer" }} size="large" />
                  </Dropdown>
                </div>

                <Divider margin="8px" />
                <Descriptions align="left" data={expandData} />
              </div>
            );
          })}
        </div>
      );
    },
    [jobList, callAction],
  );

  const handleRow: OnRow<JobDetail> = (_, index = 0) => {
    if (index % 2 === 0) {
      return {
        style: {
          background: "var(--semi-color-fill-0)",
        },
      };
    }
    return {};
  };

  const invalidJobCount = data.filter((u) => !u.triggers?.length).length;

  return (
    <>
      <Modal
        title="手动执行 - 自定义数据"
        visible={manualRunModal.visible}
        onCancel={() =>
          setManualRunModal({
            visible: false,
            jobId: "",
            triggerId: "",
            json: "",
          })
        }
        onOk={handleRunWithCustomData}
        okText="执行"
        cancelText="取消"
        closeOnEsc
      >
        <div style={{ marginBottom: 12 }}>
          JobId：<Tag>{manualRunModal.jobId}</Tag> &nbsp; TriggerId：
          <Tag>{manualRunModal.triggerId}</Tag>
        </div>
        <TextArea
          placeholder="请输入 JSON 格式的自定义数据（可选）"
          value={manualRunModal.json}
          onChange={(value) =>
            setManualRunModal((prev) => ({ ...prev, json: value }))
          }
          rows={4}
          autosize
        />
      </Modal>

      <Tabs
        type="card"
        activeKey={activeTab}
        onChange={(key) => {
          setActiveTab(key);
          syncUrlParams(
            key,
            pagination.currentPage,
            pagination.pageSize,
            words,
          );
        }}
      >
        <TabPane
          tab={
            <span>
              <IconList />
              作业列表
            </span>
          }
          itemKey="list"
        >
          <div
            style={{
              border: "1px solid var(--semi-color-border)",
              borderRadius: "10px",
            }}
          >
            <Input
              prefix={<IconSearch />}
              showClear
              placeholder="搜索关键字..."
              value={words}
              onChange={(val) => {
                const newVal = val || "";
                setWords(newVal);
                setPagination((prev) => ({ ...prev, currentPage: 1 }));
                syncUrlParams(activeTab, 1, pagination.pageSize, newVal);
              }}
              autoFocus
            />

            <Table
              rowKey="jobId"
              columns={columns}
              dataSource={paginatedData}
              onRow={handleRow}
              expandedRowRender={expandRowRender}
              pagination={{
                size: "small",
                currentPage: pagination.currentPage,
                pageSize: pagination.pageSize,
                total: data.length,
                onPageChange: (page: number) => {
                  syncUrlParams(activeTab, page, pagination.pageSize, words);
                  setPagination((prev) => ({ ...prev, currentPage: page }));
                },
                onPageSizeChange: (pageSize: number) => {
                  syncUrlParams(activeTab, 1, pageSize, words);
                  setPagination((prev) => ({
                    ...prev,
                    pageSize,
                    currentPage: 1,
                  }));
                },
                formatPageText: () => (
                  <Typography.Paragraph
                    type="secondary"
                    style={{ padding: "0 10px" }}
                  >
                    {words.trim().length > 0 ? (
                      <>
                        搜索 "<b>{words.trim()}</b>" 共 <b>{jobList.length}</b>{" "}
                        项结果。
                      </>
                    ) : (
                      <>
                        共有 <b>{data.length}</b> 项作业任务
                        {invalidJobCount > 0 && (
                          <>
                            ，其中 <b>{invalidJobCount}</b> 项未设置触发器
                          </>
                        )}
                        。
                      </>
                    )}
                  </Typography.Paragraph>
                ),
              }}
              resizable
              bordered
              expandRowByClick
              expandAllRows={apiconfig.defaultExpandAllJobs === "true"}
              rowExpandable={(jobDetail) =>
                !!(
                  jobDetail?.jobId &&
                  jobList.find((u) => u.jobDetail?.jobId === jobDetail?.jobId)
                    ?.triggers?.length
                )
              }
            />
          </div>
        </TabPane>
        <TabPane
          tab={
            <span>
              <IconCalendar />
              运行记录
            </span>
          }
          itemKey="timelines"
        >
          <div style={{ color: "var(--semi-color-text-0)" }}>
            {allTimelines.map((timeline, i) => (
              <div
                key={`${timeline.jobId || ""}_${timeline.triggerId || ""}_${i}`}
                style={{ marginBottom: 14, fontSize: 14 }}
                className={clsx(
                  styles.timelineItem,
                  mode === "dark" && styles.dark,
                )}
              >
                <IconSmallTriangleRight
                  style={{
                    color: "var(--semi-color-text-2)",
                  }}
                />
                <Tag
                  size="large"
                  color="green"
                  type="light"
                  style={{ fontWeight: 600 }}
                >
                  {timeline.jobId}
                </Tag>{" "}
                <Tag size="large" color="green" type="light">
                  {timeline.triggerId}
                </Tag>{" "}
                <Tag color="grey" type="light">
                  {dayTime(timeline.lastRunTime).format("YYYY/MM/DD HH:mm:ss")}(
                  {dayFromNow(timeline.lastRunTime)})
                </Tag>{" "}
                第{" "}
                <Tag color="green" type="light">
                  {timeline.numberOfRuns}
                </Tag>{" "}
                次运行，耗时{" "}
                <Tooltip
                  content={`${timeline.elapsedTime}ms`}
                  zIndex={10000000001}
                >
                  <span>
                    <Tag color="lime" type="light">
                      {formatDuration(timeline.elapsedTime || 0)}
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
                        style={{ padding: 10, width: 400 }}
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
              </div>
            ))}
          </div>
        </TabPane>
        <TabPane
          tab={
            <span>
              <IconCopyAdd />
              添加作业
            </span>
          }
          itemKey="addjob"
        >
          <JsonViewer
            ref={jsonviewerRef}
            height={560}
            width="100%"
            showSearch={false}
            value={defaultJsonValue}
            onChange={(v) => setJsonValue(v)}
          />
          <Space style={{ marginTop: 10 }} spacing={12}>
            <Button
              theme="solid"
              type="primary"
              onClick={handleSubmitJob}
              loading={submitting}
              disabled={submitting}
            >
              {submitting ? "提交中..." : "提交数据"}
            </Button>
            <Button
              type="tertiary"
              theme="light"
              onClick={() => jsonviewerRef.current.format()}
            >
              格式化
            </Button>
          </Space>
        </TabPane>
      </Tabs>
    </>
  );
}
