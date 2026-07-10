import { IconClockStroked } from "@douyinfe/semi-icons";
import { Space, Tag, Tooltip } from "@douyinfe/semi-ui";
import { useEffect, useState } from "react";

const CurrentTime = () => {
  const [currentTime, setCurrentTime] = useState({
    date: "",
    time: "",
    weekday: "",
  });

  const formatDateTime = (date: Date) => {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    const hours = String(date.getHours()).padStart(2, "0");
    const minutes = String(date.getMinutes()).padStart(2, "0");
    const seconds = String(date.getSeconds()).padStart(2, "0");
    const weekdays = [
      "星期日",
      "星期一",
      "星期二",
      "星期三",
      "星期四",
      "星期五",
      "星期六",
    ];
    return {
      date: `${year}/${month}/${day}`,
      time: `${hours}:${minutes}:${seconds}`,
      weekday: weekdays[date.getDay()],
    };
  };

  useEffect(() => {
    setCurrentTime(formatDateTime(new Date()));
    const timer = setInterval(
      () => setCurrentTime(formatDateTime(new Date())),
      1000,
    );
    return () => clearInterval(timer);
  }, []);

  return (
    <Tooltip content="当前时间">
      <Tag
        style={{ marginRight: 16 }}
        size="large"
        shape="circle"
        color="light-blue"
        prefixIcon={<IconClockStroked />}
      >
        <Space spacing={4}>
          <span>{currentTime.date}</span>
          <span>{currentTime.time}</span>
          <span>{currentTime.weekday}</span>
        </Space>
      </Tag>
    </Tooltip>
  );
};

export default CurrentTime;
