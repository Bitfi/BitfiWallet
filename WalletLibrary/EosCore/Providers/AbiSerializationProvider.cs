using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WalletLibrary.EosCore.Helpers;

namespace WalletLibrary.EosCore.Providers
{
  /// <summary>
  /// Serialize / deserialize transaction and fields using a Abi schema
  /// https://developers.eos.io/eosio-home/docs/the-abi
  /// </summary>
  public class AbiSerializationProvider
  {
    public static readonly Abi eosAbi = JsonConvert.DeserializeObject<Abi>("{\"version\":\"eosio::abi / 1.1\",\"types\":[],\"structs\":[{\"name\":\"abi_hash\",\"base\":\"\",\"fields\":[{\"name\":\"owner\",\"type\":\"name\"},{\"name\":\"hash\",\"type\":\"checksum256\"}]},{\"name\":\"activate\",\"base\":\"\",\"fields\":[{\"name\":\"feature_digest\",\"type\":\"checksum256\"}]},{\"name\":\"authority\",\"base\":\"\",\"fields\":[{\"name\":\"threshold\",\"type\":\"uint32\"},{\"name\":\"keys\",\"type\":\"key_weight[]\"},{\"name\":\"accounts\",\"type\":\"permission_level_weight[]\"},{\"name\":\"waits\",\"type\":\"wait_weight[]\"}]},{\"name\":\"bid_refund\",\"base\":\"\",\"fields\":[{\"name\":\"bidder\",\"type\":\"name\"},{\"name\":\"amount\",\"type\":\"asset\"}]},{\"name\":\"bidname\",\"base\":\"\",\"fields\":[{\"name\":\"bidder\",\"type\":\"name\"},{\"name\":\"newname\",\"type\":\"name\"},{\"name\":\"bid\",\"type\":\"asset\"}]},{\"name\":\"bidrefund\",\"base\":\"\",\"fields\":[{\"name\":\"bidder\",\"type\":\"name\"},{\"name\":\"newname\",\"type\":\"name\"}]},{\"name\":\"block_header\",\"base\":\"\",\"fields\":[{\"name\":\"timestamp\",\"type\":\"uint32\"},{\"name\":\"producer\",\"type\":\"name\"},{\"name\":\"confirmed\",\"type\":\"uint16\"},{\"name\":\"previous\",\"type\":\"checksum256\"},{\"name\":\"transaction_mroot\",\"type\":\"checksum256\"},{\"name\":\"action_mroot\",\"type\":\"checksum256\"},{\"name\":\"schedule_version\",\"type\":\"uint32\"},{\"name\":\"new_producers\",\"type\":\"producer_schedule ? \"}]},{\"name\":\"blockchain_parameters\",\"base\":\"\",\"fields\":[{\"name\":\"max_block_net_usage\",\"type\":\"uint64\"},{\"name\":\"target_block_net_usage_pct\",\"type\":\"uint32\"},{\"name\":\"max_transaction_net_usage\",\"type\":\"uint32\"},{\"name\":\"base_per_transaction_net_usage\",\"type\":\"uint32\"},{\"name\":\"net_usage_leeway\",\"type\":\"uint32\"},{\"name\":\"context_free_discount_net_usage_num\",\"type\":\"uint32\"},{\"name\":\"context_free_discount_net_usage_den\",\"type\":\"uint32\"},{\"name\":\"max_block_cpu_usage\",\"type\":\"uint32\"},{\"name\":\"target_block_cpu_usage_pct\",\"type\":\"uint32\"},{\"name\":\"max_transaction_cpu_usage\",\"type\":\"uint32\"},{\"name\":\"min_transaction_cpu_usage\",\"type\":\"uint32\"},{\"name\":\"max_transaction_lifetime\",\"type\":\"uint32\"},{\"name\":\"deferred_trx_expiration_window\",\"type\":\"uint32\"},{\"name\":\"max_transaction_delay\",\"type\":\"uint32\"},{\"name\":\"max_inline_action_size\",\"type\":\"uint32\"},{\"name\":\"max_inline_action_depth\",\"type\":\"uint16\"},{\"name\":\"max_authority_depth\",\"type\":\"uint16\"}]},{\"name\":\"buyram\",\"base\":\"\",\"fields\":[{\"name\":\"payer\",\"type\":\"name\"},{\"name\":\"receiver\",\"type\":\"name\"},{\"name\":\"quant\",\"type\":\"asset\"}]},{\"name\":\"buyrambytes\",\"base\":\"\",\"fields\":[{\"name\":\"payer\",\"type\":\"name\"},{\"name\":\"receiver\",\"type\":\"name\"},{\"name\":\"bytes\",\"type\":\"uint32\"}]},{\"name\":\"buyrex\",\"base\":\"\",\"fields\":[{\"name\":\"from\",\"type\":\"name\"},{\"name\":\"amount\",\"type\":\"asset\"}]},{\"name\":\"canceldelay\",\"base\":\"\",\"fields\":[{\"name\":\"canceling_auth\",\"type\":\"permission_level\"},{\"name\":\"trx_id\",\"type\":\"checksum256\"}]},{\"name\":\"claimrewards\",\"base\":\"\",\"fields\":[{\"name\":\"owner\",\"type\":\"name\"}]},{\"name\":\"closerex\",\"base\":\"\",\"fields\":[{\"name\":\"owner\",\"type\":\"name\"}]},{\"name\":\"cnclrexorder\",\"base\":\"\",\"fields\":[{\"name\":\"owner\",\"type\":\"name\"}]},{\"name\":\"connector\",\"base\":\"\",\"fields\":[{\"name\":\"balance\",\"type\":\"asset\"},{\"name\":\"weight\",\"type\":\"float64\"}]},{\"name\":\"consolidate\",\"base\":\"\",\"fields\":[{\"name\":\"owner\",\"type\":\"name\"}]},{\"name\":\"defcpuloan\",\"base\":\"\",\"fields\":[{\"name\":\"from\",\"type\":\"name\"},{\"name\":\"loan_num\",\"type\":\"uint64\"},{\"name\":\"amount\",\"type\":\"asset\"}]},{\"name\":\"defnetloan\",\"base\":\"\",\"fields\":[{\"name\":\"from\",\"type\":\"name\"},{\"name\":\"loan_num\",\"type\":\"uint64\"},{\"name\":\"amount\",\"type\":\"asset\"}]},{\"name\":\"delegatebw\",\"base\":\"\",\"fields\":[{\"name\":\"from\",\"type\":\"name\"},{\"name\":\"receiver\",\"type\":\"name\"},{\"name\":\"stake_net_quantity\",\"type\":\"asset\"},{\"name\":\"stake_cpu_quantity\",\"type\":\"asset\"},{\"name\":\"transfer\",\"type\":\"bool\"}]},{\"name\":\"delegated_bandwidth\",\"base\":\"\",\"fields\":[{\"name\":\"from\",\"type\":\"name\"},{\"name\":\"to\",\"type\":\"name\"},{\"name\":\"net_weight\",\"type\":\"asset\"},{\"name\":\"cpu_weight\",\"type\":\"asset\"}]},{\"name\":\"deleteauth\",\"base\":\"\",\"fields\":[{\"name\":\"account\",\"type\":\"name\"},{\"name\":\"permission\",\"type\":\"name\"}]},{\"name\":\"deposit\",\"base\":\"\",\"fields\":[{\"name\":\"owner\",\"type\":\"name\"},{\"name\":\"amount\",\"type\":\"asset\"}]},{\"name\":\"eosio_global_state\",\"base\":\"blockchain_parameters\",\"fields\":[{\"name\":\"max_ram_size\",\"type\":\"uint64\"},{\"name\":\"total_ram_bytes_reserved\",\"type\":\"uint64\"},{\"name\":\"total_ram_stake\",\"type\":\"int64\"},{\"name\":\"last_producer_schedule_update\",\"type\":\"block_timestamp_type\"},{\"name\":\"last_pervote_bucket_fill\",\"type\":\"time_point\"},{\"name\":\"pervote_bucket\",\"type\":\"int64\"},{\"name\":\"perblock_bucket\",\"type\":\"int64\"},{\"name\":\"total_unpaid_blocks\",\"type\":\"uint32\"},{\"name\":\"total_activated_stake\",\"type\":\"int64\"},{\"name\":\"thresh_activated_stake_time\",\"type\":\"time_point\"},{\"name\":\"last_producer_schedule_size\",\"type\":\"uint16\"},{\"name\":\"total_producer_vote_weight\",\"type\":\"float64\"},{\"name\":\"last_name_close\",\"type\":\"block_timestamp_type\"}]},{\"name\":\"eosio_global_state2\",\"base\":\"\",\"fields\":[{\"name\":\"new_ram_per_block\",\"type\":\"uint16\"},{\"name\":\"last_ram_increase\",\"type\":\"block_timestamp_type\"},{\"name\":\"last_block_num\",\"type\":\"block_timestamp_type\"},{\"name\":\"total_producer_votepay_share\",\"type\":\"float64\"},{\"name\":\"revision\",\"type\":\"uint8\"}]},{\"name\":\"eosio_global_state3\",\"base\":\"\",\"fields\":[{\"name\":\"last_vpay_state_update\",\"type\":\"time_point\"},{\"name\":\"total_vpay_share_change_rate\",\"type\":\"float64\"}]},{\"name\":\"eosio_global_state4\",\"base\":\"\",\"fields\":[{\"name\":\"continuous_rate\",\"type\":\"float64\"},{\"name\":\"inflation_pay_factor\",\"type\":\"int64\"},{\"name\":\"votepay_factor\",\"type\":\"int64\"}]},{\"name\":\"exchange_state\",\"base\":\"\",\"fields\":[{\"name\":\"supply\",\"type\":\"asset\"},{\"name\":\"base\",\"type\":\"connector\"},{\"name\":\"quote\",\"type\":\"connector\"}]},{\"name\":\"fundcpuloan\",\"base\":\"\",\"fields\":[{\"name\":\"from\",\"type\":\"name\"},{\"name\":\"loan_num\",\"type\":\"uint64\"},{\"name\":\"payment\",\"type\":\"asset\"}]},{\"name\":\"fundnetloan\",\"base\":\"\",\"fields\":[{\"name\":\"from\",\"type\":\"name\"},{\"name\":\"loan_num\",\"type\":\"uint64\"},{\"name\":\"payment\",\"type\":\"asset\"}]},{\"name\":\"init\",\"base\":\"\",\"fields\":[{\"name\":\"version\",\"type\":\"varuint32\"},{\"name\":\"core\",\"type\":\"symbol\"}]},{\"name\":\"key_weight\",\"base\":\"\",\"fields\":[{\"name\":\"key\",\"type\":\"public_key\"},{\"name\":\"weight\",\"type\":\"uint16\"}]},{\"name\":\"linkauth\",\"base\":\"\",\"fields\":[{\"name\":\"account\",\"type\":\"name\"},{\"name\":\"code\",\"type\":\"name\"},{\"name\":\"type\",\"type\":\"name\"},{\"name\":\"requirement\",\"type\":\"name\"}]},{\"name\":\"mvfrsavings\",\"base\":\"\",\"fields\":[{\"name\":\"owner\",\"type\":\"name\"},{\"name\":\"rex\",\"type\":\"asset\"}]},{\"name\":\"mvtosavings\",\"base\":\"\",\"fields\":[{\"name\":\"owner\",\"type\":\"name\"},{\"name\":\"rex\",\"type\":\"asset\"}]},{\"name\":\"name_bid\",\"base\":\"\",\"fields\":[{\"name\":\"newname\",\"type\":\"name\"},{\"name\":\"high_bidder\",\"type\":\"name\"},{\"name\":\"high_bid\",\"type\":\"int64\"},{\"name\":\"last_bid_time\",\"type\":\"time_point\"}]},{\"name\":\"newaccount\",\"base\":\"\",\"fields\":[{\"name\":\"creator\",\"type\":\"name\"},{\"name\":\"name\",\"type\":\"name\"},{\"name\":\"owner\",\"type\":\"authority\"},{\"name\":\"active\",\"type\":\"authority\"}]},{\"name\":\"onblock\",\"base\":\"\",\"fields\":[{\"name\":\"header\",\"type\":\"block_header\"}]},{\"name\":\"onerror\",\"base\":\"\",\"fields\":[{\"name\":\"sender_id\",\"type\":\"uint128\"},{\"name\":\"sent_trx\",\"type\":\"bytes\"}]},{\"name\":\"pair_time_point_sec_int64\",\"base\":\"\",\"fields\":[{\"name\":\"first\",\"type\":\"time_point_sec\"},{\"name\":\"second\",\"type\":\"int64\"}]},{\"name\":\"permission_level\",\"base\":\"\",\"fields\":[{\"name\":\"actor\",\"type\":\"name\"},{\"name\":\"permission\",\"type\":\"name\"}]},{\"name\":\"permission_level_weight\",\"base\":\"\",\"fields\":[{\"name\":\"permission\",\"type\":\"permission_level\"},{\"name\":\"weight\",\"type\":\"uint16\"}]},{\"name\":\"producer_info\",\"base\":\"\",\"fields\":[{\"name\":\"owner\",\"type\":\"name\"},{\"name\":\"total_votes\",\"type\":\"float64\"},{\"name\":\"producer_key\",\"type\":\"public_key\"},{\"name\":\"is_active\",\"type\":\"bool\"},{\"name\":\"url\",\"type\":\"string\"},{\"name\":\"unpaid_blocks\",\"type\":\"uint32\"},{\"name\":\"last_claim_time\",\"type\":\"time_point\"},{\"name\":\"location\",\"type\":\"uint16\"}]},{\"name\":\"producer_info2\",\"base\":\"\",\"fields\":[{\"name\":\"owner\",\"type\":\"name\"},{\"name\":\"votepay_share\",\"type\":\"float64\"},{\"name\":\"last_votepay_share_update\",\"type\":\"time_point\"}]},{\"name\":\"producer_key\",\"base\":\"\",\"fields\":[{\"name\":\"producer_name\",\"type\":\"name\"},{\"name\":\"block_signing_key\",\"type\":\"public_key\"}]},{\"name\":\"producer_schedule\",\"base\":\"\",\"fields\":[{\"name\":\"version\",\"type\":\"uint32\"},{\"name\":\"producers\",\"type\":\"producer_key[]\"}]},{\"name\":\"refund\",\"base\":\"\",\"fields\":[{\"name\":\"owner\",\"type\":\"name\"}]},{\"name\":\"refund_request\",\"base\":\"\",\"fields\":[{\"name\":\"owner\",\"type\":\"name\"},{\"name\":\"request_time\",\"type\":\"time_point_sec\"},{\"name\":\"net_amount\",\"type\":\"asset\"},{\"name\":\"cpu_amount\",\"type\":\"asset\"}]},{\"name\":\"regproducer\",\"base\":\"\",\"fields\":[{\"name\":\"producer\",\"type\":\"name\"},{\"name\":\"producer_key\",\"type\":\"public_key\"},{\"name\":\"url\",\"type\":\"string\"},{\"name\":\"location\",\"type\":\"uint16\"}]},{\"name\":\"regproxy\",\"base\":\"\",\"fields\":[{\"name\":\"proxy\",\"type\":\"name\"},{\"name\":\"isproxy\",\"type\":\"bool\"}]},{\"name\":\"rentcpu\",\"base\":\"\",\"fields\":[{\"name\":\"from\",\"type\":\"name\"},{\"name\":\"receiver\",\"type\":\"name\"},{\"name\":\"loan_payment\",\"type\":\"asset\"},{\"name\":\"loan_fund\",\"type\":\"asset\"}]},{\"name\":\"rentnet\",\"base\":\"\",\"fields\":[{\"name\":\"from\",\"type\":\"name\"},{\"name\":\"receiver\",\"type\":\"name\"},{\"name\":\"loan_payment\",\"type\":\"asset\"},{\"name\":\"loan_fund\",\"type\":\"asset\"}]},{\"name\":\"rex_balance\",\"base\":\"\",\"fields\":[{\"name\":\"version\",\"type\":\"uint8\"},{\"name\":\"owner\",\"type\":\"name\"},{\"name\":\"vote_stake\",\"type\":\"asset\"},{\"name\":\"rex_balance\",\"type\":\"asset\"},{\"name\":\"matured_rex\",\"type\":\"int64\"},{\"name\":\"rex_maturities\",\"type\":\"pair_time_point_sec_int64[]\"}]},{\"name\":\"rex_fund\",\"base\":\"\",\"fields\":[{\"name\":\"version\",\"type\":\"uint8\"},{\"name\":\"owner\",\"type\":\"name\"},{\"name\":\"balance\",\"type\":\"asset\"}]},{\"name\":\"rex_loan\",\"base\":\"\",\"fields\":[{\"name\":\"version\",\"type\":\"uint8\"},{\"name\":\"from\",\"type\":\"name\"},{\"name\":\"receiver\",\"type\":\"name\"},{\"name\":\"payment\",\"type\":\"asset\"},{\"name\":\"balance\",\"type\":\"asset\"},{\"name\":\"total_staked\",\"type\":\"asset\"},{\"name\":\"loan_num\",\"type\":\"uint64\"},{\"name\":\"expiration\",\"type\":\"time_point\"}]},{\"name\":\"rex_order\",\"base\":\"\",\"fields\":[{\"name\":\"version\",\"type\":\"uint8\"},{\"name\":\"owner\",\"type\":\"name\"},{\"name\":\"rex_requested\",\"type\":\"asset\"},{\"name\":\"proceeds\",\"type\":\"asset\"},{\"name\":\"stake_change\",\"type\":\"asset\"},{\"name\":\"order_time\",\"type\":\"time_point\"},{\"name\":\"is_open\",\"type\":\"bool\"}]},{\"name\":\"rex_pool\",\"base\":\"\",\"fields\":[{\"name\":\"version\",\"type\":\"uint8\"},{\"name\":\"total_lent\",\"type\":\"asset\"},{\"name\":\"total_unlent\",\"type\":\"asset\"},{\"name\":\"total_rent\",\"type\":\"asset\"},{\"name\":\"total_lendable\",\"type\":\"asset\"},{\"name\":\"total_rex\",\"type\":\"asset\"},{\"name\":\"namebid_proceeds\",\"type\":\"asset\"},{\"name\":\"loan_num\",\"type\":\"uint64\"}]},{\"name\":\"rexexec\",\"base\":\"\",\"fields\":[{\"name\":\"user\",\"type\":\"name\"},{\"name\":\"max\",\"type\":\"uint16\"}]},{\"name\":\"rmvproducer\",\"base\":\"\",\"fields\":[{\"name\":\"producer\",\"type\":\"name\"}]},{\"name\":\"sellram\",\"base\":\"\",\"fields\":[{\"name\":\"account\",\"type\":\"name\"},{\"name\":\"bytes\",\"type\":\"int64\"}]},{\"name\":\"sellrex\",\"base\":\"\",\"fields\":[{\"name\":\"from\",\"type\":\"name\"},{\"name\":\"rex\",\"type\":\"asset\"}]},{\"name\":\"setabi\",\"base\":\"\",\"fields\":[{\"name\":\"account\",\"type\":\"name\"},{\"name\":\"abi\",\"type\":\"bytes\"}]},{\"name\":\"setacctcpu\",\"base\":\"\",\"fields\":[{\"name\":\"account\",\"type\":\"name\"},{\"name\":\"cpu_weight\",\"type\":\"int64 ? \"}]},{\"name\":\"setacctnet\",\"base\":\"\",\"fields\":[{\"name\":\"account\",\"type\":\"name\"},{\"name\":\"net_weight\",\"type\":\"int64 ? \"}]},{\"name\":\"setacctram\",\"base\":\"\",\"fields\":[{\"name\":\"account\",\"type\":\"name\"},{\"name\":\"ram_bytes\",\"type\":\"int64 ? \"}]},{\"name\":\"setalimits\",\"base\":\"\",\"fields\":[{\"name\":\"account\",\"type\":\"name\"},{\"name\":\"ram_bytes\",\"type\":\"int64\"},{\"name\":\"net_weight\",\"type\":\"int64\"},{\"name\":\"cpu_weight\",\"type\":\"int64\"}]},{\"name\":\"setcode\",\"base\":\"\",\"fields\":[{\"name\":\"account\",\"type\":\"name\"},{\"name\":\"vmtype\",\"type\":\"uint8\"},{\"name\":\"vmversion\",\"type\":\"uint8\"},{\"name\":\"code\",\"type\":\"bytes\"}]},{\"name\":\"setinflation\",\"base\":\"\",\"fields\":[{\"name\":\"annual_rate\",\"type\":\"int64\"},{\"name\":\"inflation_pay_factor\",\"type\":\"int64\"},{\"name\":\"votepay_factor\",\"type\":\"int64\"}]},{\"name\":\"setparams\",\"base\":\"\",\"fields\":[{\"name\":\"params\",\"type\":\"blockchain_parameters\"}]},{\"name\":\"setpriv\",\"base\":\"\",\"fields\":[{\"name\":\"account\",\"type\":\"name\"},{\"name\":\"is_priv\",\"type\":\"uint8\"}]},{\"name\":\"setram\",\"base\":\"\",\"fields\":[{\"name\":\"max_ram_size\",\"type\":\"uint64\"}]},{\"name\":\"setramrate\",\"base\":\"\",\"fields\":[{\"name\":\"bytes_per_block\",\"type\":\"uint16\"}]},{\"name\":\"setrex\",\"base\":\"\",\"fields\":[{\"name\":\"balance\",\"type\":\"asset\"}]},{\"name\":\"undelegatebw\",\"base\":\"\",\"fields\":[{\"name\":\"from\",\"type\":\"name\"},{\"name\":\"receiver\",\"type\":\"name\"},{\"name\":\"unstake_net_quantity\",\"type\":\"asset\"},{\"name\":\"unstake_cpu_quantity\",\"type\":\"asset\"}]},{\"name\":\"unlinkauth\",\"base\":\"\",\"fields\":[{\"name\":\"account\",\"type\":\"name\"},{\"name\":\"code\",\"type\":\"name\"},{\"name\":\"type\",\"type\":\"name\"}]},{\"name\":\"unregprod\",\"base\":\"\",\"fields\":[{\"name\":\"producer\",\"type\":\"name\"}]},{\"name\":\"unstaketorex\",\"base\":\"\",\"fields\":[{\"name\":\"owner\",\"type\":\"name\"},{\"name\":\"receiver\",\"type\":\"name\"},{\"name\":\"from_net\",\"type\":\"asset\"},{\"name\":\"from_cpu\",\"type\":\"asset\"}]},{\"name\":\"updateauth\",\"base\":\"\",\"fields\":[{\"name\":\"account\",\"type\":\"name\"},{\"name\":\"permission\",\"type\":\"name\"},{\"name\":\"parent\",\"type\":\"name\"},{\"name\":\"auth\",\"type\":\"authority\"}]},{\"name\":\"updaterex\",\"base\":\"\",\"fields\":[{\"name\":\"owner\",\"type\":\"name\"}]},{\"name\":\"updtrevision\",\"base\":\"\",\"fields\":[{\"name\":\"revision\",\"type\":\"uint8\"}]},{\"name\":\"user_resources\",\"base\":\"\",\"fields\":[{\"name\":\"owner\",\"type\":\"name\"},{\"name\":\"net_weight\",\"type\":\"asset\"},{\"name\":\"cpu_weight\",\"type\":\"asset\"},{\"name\":\"ram_bytes\",\"type\":\"int64\"}]},{\"name\":\"voteproducer\",\"base\":\"\",\"fields\":[{\"name\":\"voter\",\"type\":\"name\"},{\"name\":\"proxy\",\"type\":\"name\"},{\"name\":\"producers\",\"type\":\"name[]\"}]},{\"name\":\"voter_info\",\"base\":\"\",\"fields\":[{\"name\":\"owner\",\"type\":\"name\"},{\"name\":\"proxy\",\"type\":\"name\"},{\"name\":\"producers\",\"type\":\"name[]\"},{\"name\":\"staked\",\"type\":\"int64\"},{\"name\":\"last_vote_weight\",\"type\":\"float64\"},{\"name\":\"proxied_vote_weight\",\"type\":\"float64\"},{\"name\":\"is_proxy\",\"type\":\"bool\"},{\"name\":\"flags1\",\"type\":\"uint32\"},{\"name\":\"reserved2\",\"type\":\"uint32\"},{\"name\":\"reserved3\",\"type\":\"asset\"}]},{\"name\":\"wait_weight\",\"base\":\"\",\"fields\":[{\"name\":\"wait_sec\",\"type\":\"uint32\"},{\"name\":\"weight\",\"type\":\"uint16\"}]},{\"name\":\"withdraw\",\"base\":\"\",\"fields\":[{\"name\":\"owner\",\"type\":\"name\"},{\"name\":\"amount\",\"type\":\"asset\"}]}],\"actions\":[{\"name\":\"activate\",\"type\":\"activate\",\"ricardian_contract\":\"\"},{\"name\":\"bidname\",\"type\":\"bidname\",\"ricardian_contract\":\"\"},{\"name\":\"bidrefund\",\"type\":\"bidrefund\",\"ricardian_contract\":\"\"},{\"name\":\"buyram\",\"type\":\"buyram\",\"ricardian_contract\":\"\"},{\"name\":\"buyrambytes\",\"type\":\"buyrambytes\",\"ricardian_contract\":\"\"},{\"name\":\"buyrex\",\"type\":\"buyrex\",\"ricardian_contract\":\"\"},{\"name\":\"canceldelay\",\"type\":\"canceldelay\",\"ricardian_contract\":\"\"},{\"name\":\"claimrewards\",\"type\":\"claimrewards\",\"ricardian_contract\":\"\"},{\"name\":\"closerex\",\"type\":\"closerex\",\"ricardian_contract\":\"\"},{\"name\":\"cnclrexorder\",\"type\":\"cnclrexorder\",\"ricardian_contract\":\"\"},{\"name\":\"consolidate\",\"type\":\"consolidate\",\"ricardian_contract\":\"\"},{\"name\":\"defcpuloan\",\"type\":\"defcpuloan\",\"ricardian_contract\":\"\"},{\"name\":\"defnetloan\",\"type\":\"defnetloan\",\"ricardian_contract\":\"\"},{\"name\":\"delegatebw\",\"type\":\"delegatebw\",\"ricardian_contract\":\"\"},{\"name\":\"deleteauth\",\"type\":\"deleteauth\",\"ricardian_contract\":\"\"},{\"name\":\"deposit\",\"type\":\"deposit\",\"ricardian_contract\":\"\"},{\"name\":\"fundcpuloan\",\"type\":\"fundcpuloan\",\"ricardian_contract\":\"\"},{\"name\":\"fundnetloan\",\"type\":\"fundnetloan\",\"ricardian_contract\":\"\"},{\"name\":\"init\",\"type\":\"init\",\"ricardian_contract\":\"\"},{\"name\":\"linkauth\",\"type\":\"linkauth\",\"ricardian_contract\":\"\"},{\"name\":\"mvfrsavings\",\"type\":\"mvfrsavings\",\"ricardian_contract\":\"\"},{\"name\":\"mvtosavings\",\"type\":\"mvtosavings\",\"ricardian_contract\":\"\"},{\"name\":\"newaccount\",\"type\":\"newaccount\",\"ricardian_contract\":\"\"},{\"name\":\"onblock\",\"type\":\"onblock\",\"ricardian_contract\":\"\"},{\"name\":\"onerror\",\"type\":\"onerror\",\"ricardian_contract\":\"\"},{\"name\":\"refund\",\"type\":\"refund\",\"ricardian_contract\":\"\"},{\"name\":\"regproducer\",\"type\":\"regproducer\",\"ricardian_contract\":\"\"},{\"name\":\"regproxy\",\"type\":\"regproxy\",\"ricardian_contract\":\"\"},{\"name\":\"rentcpu\",\"type\":\"rentcpu\",\"ricardian_contract\":\"\"},{\"name\":\"rentnet\",\"type\":\"rentnet\",\"ricardian_contract\":\"\"},{\"name\":\"rexexec\",\"type\":\"rexexec\",\"ricardian_contract\":\"\"},{\"name\":\"rmvproducer\",\"type\":\"rmvproducer\",\"ricardian_contract\":\"\"},{\"name\":\"sellram\",\"type\":\"sellram\",\"ricardian_contract\":\"\"},{\"name\":\"sellrex\",\"type\":\"sellrex\",\"ricardian_contract\":\"\"},{\"name\":\"setabi\",\"type\":\"setabi\",\"ricardian_contract\":\"\"},{\"name\":\"setacctcpu\",\"type\":\"setacctcpu\",\"ricardian_contract\":\"\"},{\"name\":\"setacctnet\",\"type\":\"setacctnet\",\"ricardian_contract\":\"\"},{\"name\":\"setacctram\",\"type\":\"setacctram\",\"ricardian_contract\":\"\"},{\"name\":\"setalimits\",\"type\":\"setalimits\",\"ricardian_contract\":\"\"},{\"name\":\"setcode\",\"type\":\"setcode\",\"ricardian_contract\":\"\"},{\"name\":\"setinflation\",\"type\":\"setinflation\",\"ricardian_contract\":\"\"},{\"name\":\"setparams\",\"type\":\"setparams\",\"ricardian_contract\":\"\"},{\"name\":\"setpriv\",\"type\":\"setpriv\",\"ricardian_contract\":\"\"},{\"name\":\"setram\",\"type\":\"setram\",\"ricardian_contract\":\"\"},{\"name\":\"setramrate\",\"type\":\"setramrate\",\"ricardian_contract\":\"\"},{\"name\":\"setrex\",\"type\":\"setrex\",\"ricardian_contract\":\"\"},{\"name\":\"undelegatebw\",\"type\":\"undelegatebw\",\"ricardian_contract\":\"\"},{\"name\":\"unlinkauth\",\"type\":\"unlinkauth\",\"ricardian_contract\":\"\"},{\"name\":\"unregprod\",\"type\":\"unregprod\",\"ricardian_contract\":\"\"},{\"name\":\"unstaketorex\",\"type\":\"unstaketorex\",\"ricardian_contract\":\"\"},{\"name\":\"updateauth\",\"type\":\"updateauth\",\"ricardian_contract\":\"\"},{\"name\":\"updaterex\",\"type\":\"updaterex\",\"ricardian_contract\":\"\"},{\"name\":\"updtrevision\",\"type\":\"updtrevision\",\"ricardian_contract\":\"\"},{\"name\":\"voteproducer\",\"type\":\"voteproducer\",\"ricardian_contract\":\"\"},{\"name\":\"withdraw\",\"type\":\"withdraw\",\"ricardian_contract\":\"\"}],\"tables\":[{\"name\":\"abihash\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"abi_hash\"},{\"name\":\"bidrefunds\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"bid_refund\"},{\"name\":\"cpuloan\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"rex_loan\"},{\"name\":\"delband\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"delegated_bandwidth\"},{\"name\":\"global\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"eosio_global_state\"},{\"name\":\"global2\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"eosio_global_state2\"},{\"name\":\"global3\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"eosio_global_state3\"},{\"name\":\"global4\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"eosio_global_state4\"},{\"name\":\"namebids\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"name_bid\"},{\"name\":\"netloan\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"rex_loan\"},{\"name\":\"producers\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"producer_info\"},{\"name\":\"producers2\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"producer_info2\"},{\"name\":\"rammarket\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"exchange_state\"},{\"name\":\"refunds\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"refund_request\"},{\"name\":\"rexbal\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"rex_balance\"},{\"name\":\"rexfund\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"rex_fund\"},{\"name\":\"rexpool\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"rex_pool\"},{\"name\":\"rexqueue\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"rex_order\"},{\"name\":\"userres\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"user_resources\"},{\"name\":\"voters\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"voter_info\"}],\"ricardian_clauses\":[],\"error_messages\":[],\"abi_extensions\":[],\"variants\":[]}");
    public static readonly Abi eosTokenAbi = JsonConvert.DeserializeObject<Abi>("{\"version\":\"eosio::abi / 1.1\",\"types\":[],\"structs\":[{\"name\":\"account\",\"base\":\"\",\"fields\":[{\"name\":\"balance\",\"type\":\"asset\"}]},{\"name\":\"close\",\"base\":\"\",\"fields\":[{\"name\":\"owner\",\"type\":\"name\"},{\"name\":\"symbol\",\"type\":\"symbol\"}]},{\"name\":\"create\",\"base\":\"\",\"fields\":[{\"name\":\"issuer\",\"type\":\"name\"},{\"name\":\"maximum_supply\",\"type\":\"asset\"}]},{\"name\":\"currency_stats\",\"base\":\"\",\"fields\":[{\"name\":\"supply\",\"type\":\"asset\"},{\"name\":\"max_supply\",\"type\":\"asset\"},{\"name\":\"issuer\",\"type\":\"name\"}]},{\"name\":\"issue\",\"base\":\"\",\"fields\":[{\"name\":\"to\",\"type\":\"name\"},{\"name\":\"quantity\",\"type\":\"asset\"},{\"name\":\"memo\",\"type\":\"string\"}]},{\"name\":\"open\",\"base\":\"\",\"fields\":[{\"name\":\"owner\",\"type\":\"name\"},{\"name\":\"symbol\",\"type\":\"symbol\"},{\"name\":\"ram_payer\",\"type\":\"name\"}]},{\"name\":\"retire\",\"base\":\"\",\"fields\":[{\"name\":\"quantity\",\"type\":\"asset\"},{\"name\":\"memo\",\"type\":\"string\"}]},{\"name\":\"transfer\",\"base\":\"\",\"fields\":[{\"name\":\"from\",\"type\":\"name\"},{\"name\":\"to\",\"type\":\"name\"},{\"name\":\"quantity\",\"type\":\"asset\"},{\"name\":\"memo\",\"type\":\"string\"}]}],\"actions\":[{\"name\":\"close\",\"type\":\"close\",\"ricardian_contract\":\"\"},{\"name\":\"create\",\"type\":\"create\",\"ricardian_contract\":\"\"},{\"name\":\"issue\",\"type\":\"issue\",\"ricardian_contract\":\"\"},{\"name\":\"open\",\"type\":\"open\",\"ricardian_contract\":\"\"},{\"name\":\"retire\",\"type\":\"retire\",\"ricardian_contract\":\"\"},{\"name\":\"transfer\",\"type\":\"transfer\",\"ricardian_contract\":\"\"}],\"tables\":[{\"name\":\"accounts\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"account\"},{\"name\":\"stat\",\"index_type\":\"i64\",\"key_names\":[],\"key_types\":[],\"type\":\"currency_stats\"}],\"ricardian_clauses\":[],\"error_messages\":[],\"abi_extensions\":[],\"variants\":[]}");
    private delegate object ReaderDelegate(byte[] data, ref int readIndex);
    private Dictionary<string, ReaderDelegate> TypeReaders { get; set; }

    /// <summary>
    /// Construct abi serialization provided using EOS api
    /// </summary>
    /// <param name="api"></param>
    public AbiSerializationProvider()
    {
      TypeReaders = new Dictionary<string, ReaderDelegate>()
            {
                {"int8",                 ReadByte               },
                {"uint8",                ReadByte               },
                {"int16",                ReadUint16             },
                {"uint16",               ReadUint16             },
                {"int32",                ReadUint32             },
                {"uint32",               ReadUint32             },
                {"int64",                ReadInt64              },
                {"uint64",               ReadUint64             },
                {"int128",               ReadInt128             },
                {"uint128",              ReadUInt128            },
                {"varuint32",            ReadVarUint32          },
                {"varint32",             ReadVarInt32           },
                {"float32",              ReadFloat32            },
                {"float64",              ReadFloat64            },
                {"float128",             ReadFloat128           },
                {"bytes",                ReadBytes              },
                {"bool",                 ReadBool               },
                {"string",               ReadString             },
                {"name",                 ReadName               },
                {"asset",                ReadAsset              },
                {"time_point",           ReadTimePoint          },
                {"time_point_sec",       ReadTimePointSec       },
                {"block_timestamp_type", ReadBlockTimestampType },
                {"symbol_code",          ReadSymbolCode         },
                {"symbol",               ReadSymbolString       },
                //{"checksum160",          ReadChecksum160        },
                //{"checksum256",          ReadChecksum256        },
                //{"checksum512",          ReadChecksum512        },
                //{"public_key",           ReadPublicKey          },
                //{"private_key",          ReadPrivateKey         },
                //{"signature",            ReadSignature          },
                //{"extended_asset",       ReadExtendedAsset      }
            };
    }
    
    /// <summary>
    /// Deserialize structure data as "Dictionary<string, object>"
    /// </summary>
    /// <param name="structType">struct type in abi</param>
    /// <param name="dataHex">data to deserialize</param>
    /// <param name="abi">abi schema to look for struct type</param>
    /// <returns></returns>
    public Dictionary<string, object> DeserializeStructData(string structType, string dataHex, Abi abi)
    {
      return DeserializeStructData<Dictionary<string, object>>(structType, dataHex, abi);
    }

    public T DeserializeActionData<T>(string actionName, string dataHex, Abi abi) where T : class, new()
    {
      var ls = DeserializeStructData(actionName, dataHex, abi);
      return ls.ToObject<T>();
    }

    /// <summary>
    /// Deserialize structure data with generic TStructData type
    /// </summary>
    /// <typeparam name="TStructData">deserialization struct data type</typeparam>
    /// <param name="structType">struct type in abi</param>
    /// <param name="dataHex">data to deserialize</param>
    /// <param name="abi">abi schema to look for struct type</param>
    /// <returns></returns>
    public TStructData DeserializeStructData<TStructData>(string structType, string dataHex, Abi abi)
    {
      var data = SerializationHelper.HexStringToByteArray(dataHex);
      var abiStruct = abi.structs.First(s => s.name == structType);
      int readIndex = 0;
      return ReadAbiStruct<TStructData>(data, abiStruct, abi, ref readIndex);
    }
    
    #region Reader Functions
    private object ReadByte(byte[] data, ref int readIndex)
    {
      return data[readIndex++];
    }

    private object ReadUint16(byte[] data, ref int readIndex)
    {
      var value = BitConverter.ToUInt16(data, readIndex);
      readIndex += 2;
      return value;
    }

    private object ReadUint32(byte[] data, ref int readIndex)
    {
      var value = BitConverter.ToUInt32(data, readIndex);
      readIndex += 4;
      return value;
    }

    private object ReadInt64(byte[] data, ref int readIndex)
    {
      var value = (Int64)BitConverter.ToUInt64(data, readIndex);
      readIndex += 8;
      return value;
    }

    private object ReadUint64(byte[] data, ref int readIndex)
    {
      var value = BitConverter.ToUInt64(data, readIndex);
      readIndex += 8;
      return value;
    }

    private object ReadInt128(byte[] data, ref int readIndex)
    {
      byte[] amount = data.Skip(readIndex).Take(16).ToArray();
      readIndex += 16;
      return SerializationHelper.SignedBinaryToDecimal(amount);
    }

    private object ReadUInt128(byte[] data, ref int readIndex)
    {
      byte[] amount = data.Skip(readIndex).Take(16).ToArray();
      readIndex += 16;
      return SerializationHelper.BinaryToDecimal(amount);
    }

    private object ReadVarUint32(byte[] data, ref int readIndex)
    {
      uint v = 0;
      int bit = 0;
      while (true)
      {
        byte b = data[readIndex++];
        v |= (uint)((b & 0x7f) << bit);
        bit += 7;
        if ((b & 0x80) == 0)
          break;
      }
      return v >> 0;
    }

    private object ReadVarInt32(byte[] data, ref int readIndex)
    {
      var v = (UInt32)ReadVarUint32(data, ref readIndex);

      if ((v & 1) != 0)
        return ((~v) >> 1) | 0x80000000;
      else
        return v >> 1;
    }

    private object ReadFloat32(byte[] data, ref int readIndex)
    {
      var value = BitConverter.ToSingle(data, readIndex);
      readIndex += 4;
      return value;
    }

    private object ReadFloat64(byte[] data, ref int readIndex)
    {
      var value = BitConverter.ToDouble(data, readIndex);
      readIndex += 8;
      return value;
    }

    private object ReadFloat128(byte[] data, ref int readIndex)
    {
      var a = data.Skip(readIndex).Take(16).ToArray();
      var value = SerializationHelper.ByteArrayToHexString(a);
      readIndex += 16;
      return value;
    }

    private object ReadBytes(byte[] data, ref int readIndex)
    {
      var size = Convert.ToInt32(ReadVarUint32(data, ref readIndex));
      var value = data.Skip(readIndex).Take(size).ToArray();
      readIndex += size;
      return value;
    }

    private object ReadBool(byte[] data, ref int readIndex)
    {
      return (byte)ReadByte(data, ref readIndex) == 1;
    }

    private object ReadString(byte[] data, ref int readIndex)
    {
      var size = Convert.ToInt32(ReadVarUint32(data, ref readIndex));
      string value = null;
      if (size > 0)
      {
        value = Encoding.UTF8.GetString(data.Skip(readIndex).Take(size).ToArray());
        readIndex += size;
      }
      return value;
    }

    private object ReadName(byte[] data, ref int readIndex)
    {
      byte[] a = data.Skip(readIndex).Take(8).ToArray();
      string result = "";

      readIndex += 8;

      for (int bit = 63; bit >= 0;)
      {
        int c = 0;
        for (int i = 0; i < 5; ++i)
        {
          if (bit >= 0)
          {
            c = (c << 1) | ((a[(int)Math.Floor((double)bit / 8)] >> (bit % 8)) & 1);
            --bit;
          }
        }
        if (c >= 6)
          result += (char)(c + 'a' - 6);
        else if (c >= 1)
          result += (char)(c + '1' - 1);
        else
          result += '.';
      }

      if (result == ".............")
        return result;

      while (result.EndsWith("."))
        result = result.Substring(0, result.Length - 1);

      return result;
    }

    private object ReadAsset(byte[] data, ref int readIndex)
    {
      byte[] amount = data.Skip(readIndex).Take(8).ToArray();

      readIndex += 8;

      var symbol = (Symbol)ReadSymbol(data, ref readIndex);
      string s = SerializationHelper.SignedBinaryToDecimal(amount, symbol.precision + 1);

      if (symbol.precision > 0)
        s = s.Substring(0, s.Length - symbol.precision) + '.' + s.Substring(s.Length - symbol.precision);

      return s + ' ' + symbol.name;
    }

    private object ReadTimePoint(byte[] data, ref int readIndex)
    {
      var low = (UInt32)ReadUint32(data, ref readIndex);
      var high = (UInt32)ReadUint32(data, ref readIndex);
      return SerializationHelper.TimePointToDate((high >> 0) * 0x100000000 + (low >> 0));
    }

    private object ReadTimePointSec(byte[] data, ref int readIndex)
    {
      var secs = (UInt32)ReadUint32(data, ref readIndex);
      return SerializationHelper.TimePointSecToDate(secs);
    }

    private object ReadBlockTimestampType(byte[] data, ref int readIndex)
    {
      var slot = (UInt32)ReadUint32(data, ref readIndex);
      return SerializationHelper.BlockTimestampToDate(slot);
    }

    private object ReadSymbolString(byte[] data, ref int readIndex)
    {
      var value = (Symbol)ReadSymbol(data, ref readIndex);
      return value.precision + ',' + value.name;
    }

    private object ReadSymbolCode(byte[] data, ref int readIndex)
    {
      byte[] a = data.Skip(readIndex).Take(8).ToArray();

      readIndex += 8;

      int len;
      for (len = 0; len < a.Length; ++len)
        if (a[len] == 0)
          break;

      return string.Join("", a.Take(len));
    }
    
    private object ReadSymbol(byte[] data, ref int readIndex)
    {
      var value = new Symbol
      {
        precision = (byte)ReadByte(data, ref readIndex)
      };

      byte[] a = data.Skip(readIndex).Take(7).ToArray();

      readIndex += 7;

      int len;
      for (len = 0; len < a.Length; ++len)
        if (a[len] == 0)
          break;

      value.name = string.Join("", a.Take(len).Select(b => (char)b));

      return value;
    }

    private object ReadPermissionLevel(byte[] data, ref int readIndex)
    {
      var value = new PermissionLevel()
      {
        actor = (string)ReadName(data, ref readIndex),
        permission = (string)ReadName(data, ref readIndex),
      };
      return value;
    }

    private string UnwrapTypeDef(Abi abi, string type)
    {
      var wtype = abi.types.FirstOrDefault(t => t.new_type_name == type);
      if (wtype != null && wtype.type != type)
      {
        return UnwrapTypeDef(abi, wtype.type);
      }

      return type;
    }

    private object ReadAbiType(byte[] data, string type, Abi abi, ref int readIndex)
    {
      object value = null;
      var uwtype = UnwrapTypeDef(abi, type);

      //optional type
      if (uwtype.EndsWith("?"))
      {
        var opt = (byte)ReadByte(data, ref readIndex);

        if (opt == 0)
        {
          return value;
        }
      }

      // array type
      if (uwtype.EndsWith("[]"))
      {
        var arrayType = uwtype.Substring(0, uwtype.Length - 2);
        var size = Convert.ToInt32(ReadVarUint32(data, ref readIndex));
        var items = new List<object>(size);

        for (int i = 0; i < size; i++)
        {
          items.Add(ReadAbiType(data, arrayType, abi, ref readIndex));
        }

        return items;
      }

      var reader = GetTypeSerializerAndCache(type, TypeReaders, abi);

      if (reader != null)
      {
        value = reader(data, ref readIndex);
      }
      else
      {
        var abiStruct = abi.structs.FirstOrDefault(s => s.name == uwtype);
        if (abiStruct != null)
        {
          value = ReadAbiStruct(data, abiStruct, abi, ref readIndex);
        }
        else
        {
          throw new Exception("Type supported writer not found.");
        }
      }

      return value;
    }

    private TSerializer GetTypeSerializerAndCache<TSerializer>(string type, Dictionary<string, TSerializer> typeSerializers, Abi abi)
    {
      TSerializer nativeSerializer;
      if (typeSerializers.TryGetValue(type, out nativeSerializer))
      {
        return nativeSerializer;
      }

      var abiTypeDef = abi.types.FirstOrDefault(t => t.new_type_name == type);

      if (abiTypeDef != null)
      {
        var serializer = GetTypeSerializerAndCache(abiTypeDef.type, typeSerializers, abi);

        if (serializer != null)
        {
          typeSerializers.Add(type, serializer);
          return serializer;
        }
      }

      return default(TSerializer);
    }

    private object ReadAbiStruct(byte[] data, AbiStruct abiStruct, Abi abi, ref int readIndex)
    {
      return ReadAbiStruct<Dictionary<string, object>>(data, abiStruct, abi, ref readIndex);
    }

    private T ReadAbiStruct<T>(byte[] data, AbiStruct abiStruct, Abi abi, ref int readIndex)
    {
      object value = default(T);

      if (!string.IsNullOrWhiteSpace(abiStruct.@base))
      {
        value = (T)ReadAbiType(data, abiStruct.@base, abi, ref readIndex);
      }
      else
      {
        value = Activator.CreateInstance(typeof(T));
      }

      if (value is IDictionary<string, object>)
      {
        var valueDict = value as IDictionary<string, object>;
        foreach (var field in abiStruct.fields)
        {
          var abiValue = ReadAbiType(data, field.type, abi, ref readIndex);
          valueDict.Add(field.name, abiValue);
        }
      }
      else
      {
        var valueType = value.GetType();
        foreach (var field in abiStruct.fields)
        {
          var abiValue = ReadAbiType(data, field.type, abi, ref readIndex);
          var fieldName = FindObjectFieldName(field.name, value.GetType());
          valueType.GetField(fieldName).SetValue(value, abiValue);
        }
      }

      return (T)value;
    }
    
    private static bool IsCollection(Type type)
    {
      return type.Name.StartsWith("List");
    }

    private static bool IsOptional(Type type)
    {
      return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    private static Type GetFirstGenericType(Type type)
    {
      return type.GetGenericArguments().First();
    }

    private static bool IsPrimitive(Type type)
    {
      return type.IsPrimitive ||
             type.Name.ToLower() == "string" ||
             type.Name.ToLower() == "byte[]";
    }

    private static string GetNormalizedReaderName(Type type, IEnumerable<Attribute> customAttributes = null)
    {
      if (customAttributes != null)
      {
        var abiFieldAttr = (AbiFieldTypeAttribute)customAttributes.FirstOrDefault(attr => attr.GetType() == typeof(AbiFieldTypeAttribute));
        if (abiFieldAttr != null)
        {
          return abiFieldAttr.AbiType;
        }

      }

      var typeName = type.Name.ToLower();

      if (typeName == "byte[]")
        return "bytes";
      else if (typeName == "boolean")
        return "bool";

      return typeName;
    }

    private string FindObjectFieldName(string name, System.Collections.IDictionary value)
    {
      if (value.Contains(name))
        return name;

      name = SerializationHelper.SnakeCaseToPascalCase(name);

      if (value.Contains(name))
        return name;

      name = SerializationHelper.PascalCaseToSnakeCase(name);

      if (value.Contains(name))
        return name;

      return null;
    }

    private string FindObjectFieldName(string name, Type objectType)
    {
      if (objectType.GetFields().Any(p => p.Name == name))
        return name;

      name = SerializationHelper.SnakeCaseToPascalCase(name);

      if (objectType.GetFields().Any(p => p.Name == name))
        return name;

      name = SerializationHelper.PascalCaseToSnakeCase(name);

      if (objectType.GetFields().Any(p => p.Name == name))
        return name;

      return null;
    }
    #endregion
  }

  [Serializable]
  public class AbiType
  {

    public string new_type_name;

    public string type;
  }

  [Serializable]
  public class PermissionLevel
  {

    public string actor;
    public string permission;
  }

  [Serializable]
  public class AbiField
  {

    public string name;

    public string type;
  }
  [Serializable]
  public class AbiStruct
  {

    public string name;

    public string @base;

    public List<AbiField> fields;
  }

  /// <summary>
  /// Data Attribute to map how the field is represented in the abi
  /// </summary>
  public class AbiFieldTypeAttribute : Attribute
  {
    public string AbiType { get; set; }

    public AbiFieldTypeAttribute(string abiType)
    {
      AbiType = abiType;
    }
  }

  [Serializable]
  public class AbiAction
  {
    [AbiFieldType("name")]
    public string name;

    public string type;

    public string ricardian_contract;
  }

  [Serializable]
  public class AbiTable
  {
    [AbiFieldType("name")]
    public string name;
    public string index_type;
    public List<string> key_names;
    public List<string> key_types;
    public string type;
  }

  [Serializable]
  public class Abi
  {
    public string version;
    public List<AbiType> types;
    public List<AbiStruct> structs;
    public List<AbiAction> actions;
    public List<AbiTable> tables;
    public List<AbiRicardianClause> ricardian_clauses;
    public List<string> error_messages;
    public List<Extension> abi_extensions;
    public List<Variant> variants;
  }

  [Serializable]
  public class AbiRicardianClause
  {
    public string id;
    public string body;
  }

  [Serializable]
  public class Extension
  {
    public UInt16 type;
    public byte[] data;
  }

  [Serializable]
  public class Symbol
  {
    public string name;
    public byte precision;
  }

  [Serializable]
  public class Variant
  {
    public string name;
    public List<string> type;
  }
}
