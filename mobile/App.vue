<script>
export default {
  onLaunch () {
    console.log('RoomTune mobile launch')
  }
}
</script>

<style>
page {
  background-color: #F4F6F8;
  color: #1F2937;
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", "PingFang SC", "Microsoft YaHei", sans-serif;
  font-size: 14px;
}
view, text, button, input, textarea, picker, scroll-view {
  box-sizing: border-box;
}
button::after {
  border: none;
}
.page {
  min-height: 100vh;
  padding: 24rpx 24rpx 50rpx;
  background-color: #F4F6F8;
}
.kicker {
  color: #2563EB;
  font-size: 20rpx;
  letter-spacing: 3rpx;
  font-weight: 600;
}
.title {
  color: #1F2937;
  font-size: 48rpx;
  line-height: 1.2;
  font-weight: 700;
  margin-top: 10rpx;
}
.subtitle {
  color: #64748B;
  font-size: 24rpx;
  line-height: 1.6;
  margin-top: 8rpx;
}
.card {
  background-color: #FFFFFF;
  border: 1rpx solid #DCE2EA;
  border-radius: 16rpx;
  padding: 24rpx;
  margin-top: 18rpx;
}
.section-title {
  color: #1F2937;
  font-size: 28rpx;
  font-weight: 600;
  line-height: 1.4;
}
.muted {
  color: #64748B;
  font-size: 22rpx;
  line-height: 1.6;
}
.small {
  color: #94A3B8;
  font-size: 20rpx;
  line-height: 1.5;
}
.btn-primary {
  background-color: #2563EB;
  color: #FFFFFF;
  border-radius: 12rpx;
  font-size: 26rpx;
  font-weight: 600;
  line-height: 80rpx;
  height: 80rpx;
  padding: 0 28rpx;
  margin: 0;
}
.btn-secondary {
  background-color: #FFFFFF;
  color: #1F2937;
  border: 1rpx solid #DCE2EA;
  border-radius: 12rpx;
  font-size: 24rpx;
  line-height: 76rpx;
  height: 76rpx;
  padding: 0 24rpx;
  margin: 0;
}
.btn-row {
  display: flex;
  gap: 16rpx;
  flex-wrap: wrap;
  margin-top: 20rpx;
}
.input-line {
  display: flex;
  align-items: center;
  min-height: 76rpx;
  border-bottom: 1rpx solid #E2E8F0;
  gap: 16rpx;
}
.input-line .label {
  width: 170rpx;
  flex: 0 0 170rpx;
  color: #64748B;
  font-size: 23rpx;
}
.input-line input, .input-line picker {
  flex: 1;
  min-width: 0;
  font-size: 25rpx;
  color: #1F2937;
}
.badge {
  padding: 5rpx 12rpx;
  border-radius: 999rpx;
  background-color: #E8F0FF;
  color: #2563EB;
  font-size: 20rpx;
}
</style>
